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

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        var normalizedUsername = dto.Username.Trim();
        if (!string.Equals(normalizedUsername, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userRepository.GetByUsernameAsync(normalizedUsername);
            if (existing != null && existing.Id != userId)
                throw new InvalidOperationException("Username is already taken");
        }

        user.Username = normalizedUsername;
        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Bio = dto.Bio.Trim();
        user.Avatar = dto.Avatar.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var followerCount = await _followRepository.GetFollowerCountAsync(userId);
        return MapToUserDto(user, followerCount, false);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect");

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        user.EmailOnComment = dto.EmailOnComment;
        user.EmailOnFollow = dto.EmailOnFollow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var followerCount = await _followRepository.GetFollowerCountAsync(userId);
        return MapToUserDto(user, followerCount, false);
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
            EmailOnComment = user.EmailOnComment,
            EmailOnFollow = user.EmailOnFollow,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            FollowerCount = followerCount,
            IsFollowedByCurrentUser = isFollowed
        };
    }
}
