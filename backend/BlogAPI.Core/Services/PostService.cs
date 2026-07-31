using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// Post service implementation
/// </summary>
public class PostService : IPostService
{
    private const int DiscoveryCandidatePoolSize = 200;

    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IFollowRepository _followRepository;
    private readonly ILikeRepository _likeRepository;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IFollowRepository followRepository,
        ILikeRepository likeRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _followRepository = followRepository;
        _likeRepository = likeRepository;
    }

    public async Task<PaginatedResponse<PostDto>> GetPublishedPostsAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? currentUserId = null, Guid? authorId = null)
    {
        var posts = await _postRepository.GetPublishedPostsAsync(pageNumber, pageSize, categoryId, tagId, search, authorId);
        var totalCount = await _postRepository.GetPublishedPostsCountAsync(categoryId, tagId, search, authorId);

        return new PaginatedResponse<PostDto>
        {
            Items = posts.Select(p => MapToPostDto(p, currentUserId)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PostDto> GetPostByIdAsync(Guid postId, Guid? currentUserId = null)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        if (currentUserId != post.UserId)
        {
            post.ViewCount++;
            await _postRepository.UpdateAsync(post);
        }

        return MapToPostDto(post, currentUserId);
    }

    public async Task<List<PostDto>> GetTrendingAsync(int count = 6, Guid? currentUserId = null)
    {
        var candidates = await _postRepository.GetTrendingCandidatesAsync(DiscoveryCandidatePoolSize);
        return RankByTrendingScore(candidates)
            .Take(count)
            .Select(p => MapToPostDto(p, currentUserId))
            .ToList();
    }

    public async Task<List<PostDto>> GetSuggestedAsync(int count = 6, Guid? currentUserId = null)
    {
        if (currentUserId is not { } userId)
            return await GetTrendingAsync(count, null);

        var candidates = await _postRepository.GetTrendingCandidatesAsync(DiscoveryCandidatePoolSize);
        var followingIds = new HashSet<Guid>(await _followRepository.GetFollowingIdsAsync(userId));
        var likedCategoryIds = new HashSet<Guid>(await _likeRepository.GetLikedCategoryIdsAsync(userId));

        var eligible = candidates
            .Where(p => p.UserId != userId)
            .Where(p => !p.Likes.Any(l => l.UserId == userId))
            .ToList();

        var personalized = eligible
            .Where(p => followingIds.Contains(p.UserId) || p.Categories.Any(c => likedCategoryIds.Contains(c.Id)))
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToList();

        if (personalized.Count < count)
        {
            var usedIds = new HashSet<Guid>(personalized.Select(p => p.Id));
            var backfill = RankByTrendingScore(eligible)
                .Where(p => !usedIds.Contains(p.Id))
                .Take(count - personalized.Count);
            personalized.AddRange(backfill);
        }

        return personalized.Select(p => MapToPostDto(p, userId)).ToList();
    }

    private static IEnumerable<Post> RankByTrendingScore(IEnumerable<Post> posts) =>
        posts.OrderByDescending(TrendingScore);

    private static double TrendingScore(Post post)
    {
        var publishedAt = post.PublishedAt ?? post.CreatedAt;
        var hoursSincePublished = Math.Max(0, (DateTime.UtcNow - publishedAt).TotalHours);
        var engagement = (post.Likes.Count * 2) + (post.Comments.Count * 3) + post.ViewCount;
        return engagement / Math.Pow(hoursSincePublished + 2, 1.5);
    }

    public async Task<PostDto> CreatePostAsync(Guid userId, CreatePostDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        var status = Enum.Parse<PostStatus>(request.Status);
        var post = new Post
        {
            UserId = userId,
            Title = request.Title,
            Excerpt = request.Excerpt,
            Content = PostContentSanitizer.Sanitize(request.Content),
            CoverImageUrl = request.CoverImageUrl,
            Status = status,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = status == PostStatus.Published ? DateTime.UtcNow : null
        };

        foreach (var category in await _categoryRepository.GetByIdsAsync(request.CategoryIds))
            post.Categories.Add(category);
        foreach (var tag in await _tagRepository.GetByIdsAsync(request.TagIds))
            post.Tags.Add(tag);

        var createdPost = await _postRepository.AddAsync(post);
        return MapToPostDto(createdPost);
    }

    public async Task<PostDto> UpdatePostAsync(Guid postId, UpdatePostDto request, Guid userId)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        if (post.UserId != userId)
            throw new UnauthorizedException("You do not have permission to update this post");

        post.Title = request.Title;
        post.Excerpt = request.Excerpt;
        post.Content = PostContentSanitizer.Sanitize(request.Content);
        post.CoverImageUrl = request.CoverImageUrl;
        post.Status = Enum.Parse<PostStatus>(request.Status);
        post.UpdatedAt = DateTime.UtcNow;

        if (request.Status == "Published" && post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        post.Categories.Clear();
        foreach (var category in await _categoryRepository.GetByIdsAsync(request.CategoryIds))
            post.Categories.Add(category);

        post.Tags.Clear();
        foreach (var tag in await _tagRepository.GetByIdsAsync(request.TagIds))
            post.Tags.Add(tag);

        await _postRepository.UpdateAsync(post);
        return MapToPostDto(post);
    }

    public async Task DeletePostAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        if (post.UserId != userId)
            throw new UnauthorizedException("You do not have permission to delete this post");

        await _postRepository.DeleteAsync(post);
    }

    public async Task<List<PostDto>> GetUserPostsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        var posts = await _postRepository.GetUserPostsAsync(userId);
        return posts.Select(p => MapToPostDto(p, userId)).ToList();
    }

    private PostDto MapToPostDto(Post post, Guid? currentUserId = null)
    {
        return new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Excerpt = post.Excerpt,
            Content = post.Content,
            CoverImageUrl = post.CoverImageUrl,
            Status = post.Status.ToString(),
            ViewCount = post.ViewCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt,
            Author = post.Author != null ? new UserDto
            {
                Id = post.Author.Id,
                Email = post.Author.Email,
                FirstName = post.Author.FirstName,
                LastName = post.Author.LastName
            } : null,
            Categories = post.Categories.Select(c => c.Name).ToList(),
            Tags = post.Tags.Select(t => t.Name).ToList(),
            LikeCount = post.Likes.Count,
            IsLikedByCurrentUser = currentUserId.HasValue && post.Likes.Any(l => l.UserId == currentUserId.Value)
        };
    }
}
