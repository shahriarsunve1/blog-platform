using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, Guid? currentUserId = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        var followerCount = await _followRepository.GetFollowerCountAsync(userId);
        var isFollowed = currentUserId.HasValue &&
            await _followRepository.GetAsync(currentUserId.Value, userId) != null;

        return MapToUserDto(user, followerCount, isFollowed);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        return MapToUserDto(user, 0, false);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userRepository.EmailExistsAsync(email);
    }

    private static UserDto MapToUserDto(User user, int followerCount, bool isFollowed)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio ?? string.Empty,
            Avatar = user.Avatar ?? string.Empty,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            FollowerCount = followerCount,
            IsFollowedByCurrentUser = isFollowed
        };
    }
}
