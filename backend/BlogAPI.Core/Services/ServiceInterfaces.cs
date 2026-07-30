using BlogAPI.Core.DTOs;

namespace BlogAPI.Core.Services;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
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
    Task<PostDto> GetPostByIdAsync(Guid postId);
    Task<PaginatedResponse<PostDto>> GetPublishedPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<PostDto>> GetUserPostsAsync(Guid userId);
    Task<PostDto> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId);
    Task DeletePostAsync(Guid postId, Guid userId);
}

/// <summary>
/// Interface for user service
/// </summary>
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
}
