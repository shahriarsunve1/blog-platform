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

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        return MapToUserDto(user);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        return MapToUserDto(user);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userRepository.EmailExistsAsync(email);
    }

    private UserDto MapToUserDto(User user)
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
            CreatedAt = user.CreatedAt
        };
    }
}
