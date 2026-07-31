using BlogAPI.Core.DTOs;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Core.Services;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<string> GenerateAccessTokenAsync(Guid userId);
    Task<string> GenerateRefreshTokenAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
}

/// <summary>
/// Interface for post service
/// </summary>
public interface IPostService
{
    Task<PostDto> CreatePostAsync(Guid userId, CreatePostDto dto);
    Task<PostDto> GetPostByIdAsync(Guid postId, Guid? currentUserId = null);
    Task<PaginatedResponse<PostDto>> GetPublishedPostsAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? currentUserId = null, Guid? authorId = null);
    Task<List<PostDto>> GetUserPostsAsync(Guid userId);
    Task<PostDto> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId);
    Task DeletePostAsync(Guid postId, Guid userId);
    Task<List<PostDto>> GetTrendingAsync(int count = 6, Guid? currentUserId = null);
    Task<List<PostDto>> GetSuggestedAsync(int count = 6, Guid? currentUserId = null);
}

/// <summary>
/// Interface for user service
/// </summary>
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId, Guid? currentUserId = null);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesDto dto);
}

/// <summary>
/// Interface for category service
/// </summary>
public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
}

/// <summary>
/// Interface for tag service
/// </summary>
public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto> CreateAsync(CreateTagDto dto);
}

/// <summary>
/// Interface for comment service
/// </summary>
public interface ICommentService
{
    Task<List<CommentDto>> GetByPostIdAsync(Guid postId);
    Task<CommentDto> CreateAsync(Guid postId, Guid userId, CreateCommentDto dto);
    Task DeleteAsync(Guid commentId, Guid userId);
}

/// <summary>
/// Interface for like service
/// </summary>
public interface ILikeService
{
    Task<int> LikeAsync(Guid postId, Guid userId);
    Task<int> UnlikeAsync(Guid postId, Guid userId);
}

/// <summary>
/// Interface for follow service
/// </summary>
public interface IFollowService
{
    Task<int> FollowAsync(Guid followerId, Guid followingId);
    Task<int> UnfollowAsync(Guid followerId, Guid followingId);
}

/// <summary>
/// Interface for admin dashboard service
/// </summary>
public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
}

/// <summary>
/// Interface for media (image) upload/storage used for embedding images in post content
/// </summary>
public interface IMediaService
{
    Task<MediaFile> UploadAsync(Guid userId, string fileName, string contentType, byte[] data);
    Task<MediaFile?> GetAsync(Guid id);
}

/// <summary>
/// Interface for transactional email sending. Implementations must never let a
/// send failure propagate - notification emails are best-effort and should
/// never break the user-facing action that triggered them.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
