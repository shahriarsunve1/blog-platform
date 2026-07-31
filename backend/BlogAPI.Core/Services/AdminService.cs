using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Enums;

namespace BlogAPI.Core.Services;

/// <summary>
/// Aggregates platform-wide stats for the admin dashboard
/// </summary>
public class AdminService : IAdminService
{
    private const int RecentItemCount = 5;

    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IFollowRepository _followRepository;

    public AdminService(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository,
        IFollowRepository followRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _likeRepository = likeRepository;
        _followRepository = followRepository;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var publishedCount = await _postRepository.CountByStatusAsync(PostStatus.Published);
        var draftCount = await _postRepository.CountByStatusAsync(PostStatus.Draft);
        var archivedCount = await _postRepository.CountByStatusAsync(PostStatus.Archived);

        var recentUsers = await _userRepository.GetRecentAsync(RecentItemCount);
        var recentPosts = await _postRepository.GetRecentAsync(RecentItemCount);

        return new AdminDashboardDto
        {
            TotalUsers = await _userRepository.CountAsync(),
            TotalPosts = publishedCount + draftCount + archivedCount,
            PublishedPosts = publishedCount,
            DraftPosts = draftCount,
            ArchivedPosts = archivedCount,
            TotalComments = await _commentRepository.CountAsync(),
            TotalLikes = await _likeRepository.CountAsync(),
            TotalFollows = await _followRepository.CountAsync(),
            RecentUsers = recentUsers.Select(u => new AdminUserSummaryDto
            {
                Id = u.Id,
                FullName = u.GetFullName(),
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList(),
            RecentPosts = recentPosts.Select(p => new AdminPostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Status = p.Status.ToString(),
                AuthorName = p.Author?.GetFullName() ?? "Unknown",
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }
}
