using BlogAPI.Core.DTOs;
using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ITagRepository> _tagRepository = new();
    private readonly Mock<IFollowRepository> _followRepository = new();
    private readonly Mock<ILikeRepository> _likeRepository = new();
    private readonly PostService _sut;

    public PostServiceTests()
    {
        _sut = new PostService(
            _postRepository.Object,
            _userRepository.Object,
            _categoryRepository.Object,
            _tagRepository.Object,
            _followRepository.Object,
            _likeRepository.Object);

        _categoryRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<Category>());
        _tagRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<Tag>());
    }

    [Fact]
    public async Task GetPublishedPostsAsync_PassesCategoryAndTagFiltersThrough()
    {
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPublishedPostsAsync(1, 10, categoryId, tagId, null, null)).ReturnsAsync(new List<Post>());
        _postRepository.Setup(r => r.GetPublishedPostsCountAsync(categoryId, tagId, null, null)).ReturnsAsync(0);

        await _sut.GetPublishedPostsAsync(1, 10, categoryId, tagId);

        _postRepository.Verify(r => r.GetPublishedPostsAsync(1, 10, categoryId, tagId, null, null), Times.Once);
        _postRepository.Verify(r => r.GetPublishedPostsCountAsync(categoryId, tagId, null, null), Times.Once);
    }

    [Fact]
    public async Task GetPublishedPostsAsync_PassesSearchTermThrough()
    {
        _postRepository.Setup(r => r.GetPublishedPostsAsync(1, 10, null, null, "angular", null)).ReturnsAsync(new List<Post>());
        _postRepository.Setup(r => r.GetPublishedPostsCountAsync(null, null, "angular", null)).ReturnsAsync(0);

        await _sut.GetPublishedPostsAsync(1, 10, search: "angular");

        _postRepository.Verify(r => r.GetPublishedPostsAsync(1, 10, null, null, "angular", null), Times.Once);
        _postRepository.Verify(r => r.GetPublishedPostsCountAsync(null, null, "angular", null), Times.Once);
    }

    [Fact]
    public async Task GetPublishedPostsAsync_PassesAuthorIdThrough()
    {
        var authorId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPublishedPostsAsync(1, 10, null, null, null, authorId)).ReturnsAsync(new List<Post>());
        _postRepository.Setup(r => r.GetPublishedPostsCountAsync(null, null, null, authorId)).ReturnsAsync(0);

        await _sut.GetPublishedPostsAsync(1, 10, authorId: authorId);

        _postRepository.Verify(r => r.GetPublishedPostsAsync(1, 10, null, null, null, authorId), Times.Once);
        _postRepository.Verify(r => r.GetPublishedPostsCountAsync(null, null, null, authorId), Times.Once);
    }

    [Fact]
    public async Task CreatePostAsync_UnknownUser_ThrowsEntityNotFound()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.CreatePostAsync(Guid.NewGuid(), new CreatePostDto { Title = "T", Excerpt = "E", Content = "C", Status = "Draft" }));
    }

    [Fact]
    public async Task CreatePostAsync_HonorsRequestedStatus()
    {
        // Regression test: CreatePostAsync used to hardcode every new post to Draft
        // regardless of what the client requested.
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

        var result = await _sut.CreatePostAsync(userId, new CreatePostDto
        {
            Title = "Title",
            Excerpt = "Excerpt",
            Content = "Content",
            Status = "Published"
        });

        Assert.Equal("Published", result.Status);
        Assert.NotNull(result.PublishedAt);
    }

    [Fact]
    public async Task CreatePostAsync_AttachesRequestedCategoriesAndTags()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _categoryRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(categoryId))))
            .ReturnsAsync(new List<Category> { new() { Id = categoryId, Name = "Technology", Slug = "technology" } });
        _tagRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(tagId))))
            .ReturnsAsync(new List<Tag> { new() { Id = tagId, Name = "Tutorial", Slug = "tutorial" } });
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

        var result = await _sut.CreatePostAsync(userId, new CreatePostDto
        {
            Title = "Title",
            Excerpt = "Excerpt",
            Content = "Content",
            Status = "Draft",
            CategoryIds = new List<Guid> { categoryId },
            TagIds = new List<Guid> { tagId }
        });

        Assert.Contains("Technology", result.Categories);
        Assert.Contains("Tutorial", result.Tags);
    }

    [Fact]
    public async Task GetPostByIdAsync_UnknownPost_ThrowsEntityNotFound()
    {
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Post?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetPostByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPostByIdAsync_ComputesLikeCountAndCurrentUserLikeStatus()
    {
        var postId = Guid.NewGuid();
        var likerId = Guid.NewGuid();
        var otherLikerId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Likes = new List<Like>
            {
                new() { PostId = postId, UserId = likerId },
                new() { PostId = postId, UserId = otherLikerId }
            }
        };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(postId)).ReturnsAsync(post);

        var likedByViewer = await _sut.GetPostByIdAsync(postId, likerId);
        var viewedAnonymously = await _sut.GetPostByIdAsync(postId, null);

        Assert.Equal(2, likedByViewer.LikeCount);
        Assert.True(likedByViewer.IsLikedByCurrentUser);
        Assert.Equal(2, viewedAnonymously.LikeCount);
        Assert.False(viewedAnonymously.IsLikedByCurrentUser);
    }

    [Fact]
    public async Task GetPostByIdAsync_ViewerIsNotAuthor_IncrementsViewCount()
    {
        var authorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId, ViewCount = 0 };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await _sut.GetPostByIdAsync(post.Id, viewerId);

        _postRepository.Verify(r => r.UpdateAsync(It.Is<Post>(p => p.ViewCount == 1)), Times.Once);
    }

    [Fact]
    public async Task GetPostByIdAsync_ViewerIsAuthor_DoesNotIncrementViewCount()
    {
        var authorId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId, ViewCount = 0 };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await _sut.GetPostByIdAsync(post.Id, authorId);

        _postRepository.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task GetPostByIdAsync_AnonymousViewer_IncrementsViewCount()
    {
        var authorId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId, ViewCount = 0 };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await _sut.GetPostByIdAsync(post.Id, null);

        _postRepository.Verify(r => r.UpdateAsync(It.Is<Post>(p => p.ViewCount == 1)), Times.Once);
    }

    [Fact]
    public async Task GetUserPostsAsync_ReturnsAllStatusesIncludingDrafts()
    {
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _postRepository.Setup(r => r.GetUserPostsAsync(userId)).ReturnsAsync(new List<Post>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Status = PostStatus.Draft },
            new() { Id = Guid.NewGuid(), UserId = userId, Status = PostStatus.Published }
        });

        var result = await _sut.GetUserPostsAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Status == "Draft");
    }

    [Fact]
    public async Task UpdatePostAsync_NotTheAuthor_ThrowsUnauthorized()
    {
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId, Status = PostStatus.Draft };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.UpdatePostAsync(post.Id, new UpdatePostDto { Title = "T", Excerpt = "E", Content = "C", Status = "Draft" }, otherUserId));
    }

    [Fact]
    public async Task UpdatePostAsync_ByAuthor_UpdatesFieldsAndReplacesTaxonomy()
    {
        var authorId = Guid.NewGuid();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = authorId,
            Title = "Old title",
            Status = PostStatus.Draft,
            Categories = new List<Category> { new() { Id = Guid.NewGuid(), Name = "Stale", Slug = "stale" } }
        };
        var newCategoryId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);
        _categoryRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(newCategoryId))))
            .ReturnsAsync(new List<Category> { new() { Id = newCategoryId, Name = "Fresh", Slug = "fresh" } });

        var result = await _sut.UpdatePostAsync(post.Id, new UpdatePostDto
        {
            Title = "New title",
            Excerpt = "E",
            Content = "C",
            Status = "Published",
            CategoryIds = new List<Guid> { newCategoryId }
        }, authorId);

        Assert.Equal("New title", result.Title);
        Assert.Equal("Published", result.Status);
        Assert.Equal(new[] { "Fresh" }, result.Categories);
    }

    [Fact]
    public async Task DeletePostAsync_NotTheAuthor_ThrowsUnauthorized()
    {
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeletePostAsync(post.Id, otherUserId));
    }

    [Fact]
    public async Task DeletePostAsync_ByAuthor_DeletesPost()
    {
        var authorId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await _sut.DeletePostAsync(post.Id, authorId);

        _postRepository.Verify(r => r.DeleteAsync(post), Times.Once);
    }

    private static Post MakeEngagedPost(DateTime publishedAt, int likes = 0, int comments = 0, int views = 0, Guid? authorId = null, List<Category>? categories = null)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = authorId ?? Guid.NewGuid(),
            Status = PostStatus.Published,
            PublishedAt = publishedAt,
            CreatedAt = publishedAt,
            ViewCount = views,
            Categories = categories ?? new List<Category>()
        };
        for (var i = 0; i < likes; i++)
            post.Likes.Add(new Like { PostId = post.Id, UserId = Guid.NewGuid() });
        for (var i = 0; i < comments; i++)
            post.Comments.Add(new Comment { PostId = post.Id, UserId = Guid.NewGuid() });
        return post;
    }

    [Fact]
    public async Task GetTrendingAsync_RanksByDecayedEngagementNotRawEngagement()
    {
        var now = DateTime.UtcNow;
        var oldButHeavilyLiked = MakeEngagedPost(now.AddDays(-30), likes: 50);
        var freshModeratelyLiked = MakeEngagedPost(now.AddHours(-1), likes: 10);
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Post> { oldButHeavilyLiked, freshModeratelyLiked });

        var result = await _sut.GetTrendingAsync(6);

        Assert.Equal(freshModeratelyLiked.Id, result[0].Id);
        Assert.Equal(oldButHeavilyLiked.Id, result[1].Id);
    }

    [Fact]
    public async Task GetTrendingAsync_ZeroEngagementTies_FallBackToPublishedAtOrder()
    {
        var now = DateTime.UtcNow;
        var posts = new List<Post>
        {
            MakeEngagedPost(now.AddHours(-1)),
            MakeEngagedPost(now.AddHours(-2)),
            MakeEngagedPost(now.AddHours(-3))
        };
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(posts);

        var result = await _sut.GetTrendingAsync(6);

        Assert.Equal(posts.Select(p => p.Id), result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetTrendingAsync_RespectsCountParameter()
    {
        var now = DateTime.UtcNow;
        var posts = Enumerable.Range(0, 10).Select(i => MakeEngagedPost(now.AddHours(-i))).ToList();
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(posts);

        var result = await _sut.GetTrendingAsync(3);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetTrendingAsync_PublishedAtNull_FallsBackToCreatedAt_DoesNotThrow()
    {
        var post = new Post { Id = Guid.NewGuid(), Status = PostStatus.Published, PublishedAt = null, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { post });

        var result = await _sut.GetTrendingAsync(6);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetSuggestedAsync_NoCurrentUserId_DelegatesToTrending()
    {
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post>());

        await _sut.GetSuggestedAsync(6, null);

        _followRepository.Verify(r => r.GetFollowingIdsAsync(It.IsAny<Guid>()), Times.Never);
        _likeRepository.Verify(r => r.GetLikedCategoryIdsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetSuggestedAsync_IncludesPostsByFollowedAuthor()
    {
        var userId = Guid.NewGuid();
        var followedAuthorId = Guid.NewGuid();
        var post = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), authorId: followedAuthorId);
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { post });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid> { followedAuthorId });
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid>());

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.Contains(result, p => p.Id == post.Id);
    }

    [Fact]
    public async Task GetSuggestedAsync_IncludesPostsSharingLikedCategory()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Technology", Slug = "technology" };
        var post = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), categories: new List<Category> { category });
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { post });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid>());
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid> { categoryId });

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.Contains(result, p => p.Id == post.Id);
    }

    [Fact]
    public async Task GetSuggestedAsync_ExcludesOwnPosts()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Technology", Slug = "technology" };
        var ownPost = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), authorId: userId, categories: new List<Category> { category });
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { ownPost });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid>());
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid> { categoryId });

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.DoesNotContain(result, p => p.Id == ownPost.Id);
    }

    [Fact]
    public async Task GetSuggestedAsync_ExcludesAlreadyLikedPosts()
    {
        var userId = Guid.NewGuid();
        var followedAuthorId = Guid.NewGuid();
        var post = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), authorId: followedAuthorId);
        post.Likes.Add(new Like { PostId = post.Id, UserId = userId });
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { post });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid> { followedAuthorId });
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid>());

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.DoesNotContain(result, p => p.Id == post.Id);
    }

    [Fact]
    public async Task GetSuggestedAsync_FewerThanCountPersonalizedCandidates_BackfillsWithTrending()
    {
        var userId = Guid.NewGuid();
        var unrelatedPost = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), likes: 5);
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { unrelatedPost });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid>());
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid>());

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.Contains(result, p => p.Id == unrelatedPost.Id);
    }

    [Fact]
    public async Task GetSuggestedAsync_Backfill_DoesNotDuplicatePersonalizedResults()
    {
        var userId = Guid.NewGuid();
        var followedAuthorId = Guid.NewGuid();
        var personalizedPost = MakeEngagedPost(DateTime.UtcNow.AddHours(-1), authorId: followedAuthorId, likes: 100);
        _postRepository.Setup(r => r.GetTrendingCandidatesAsync(It.IsAny<int>())).ReturnsAsync(new List<Post> { personalizedPost });
        _followRepository.Setup(r => r.GetFollowingIdsAsync(userId)).ReturnsAsync(new List<Guid> { followedAuthorId });
        _likeRepository.Setup(r => r.GetLikedCategoryIdsAsync(userId)).ReturnsAsync(new List<Guid>());

        var result = await _sut.GetSuggestedAsync(6, userId);

        Assert.Single(result, p => p.Id == personalizedPost.Id);
    }
}
