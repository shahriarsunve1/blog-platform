using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace BlogAPI.Core.Services;

/// <summary>
/// Follow service implementation. Following/unfollowing is idempotent, same
/// as likes. Following yourself is rejected.
/// </summary>
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public FollowService(
        IFollowRepository followRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<int> FollowAsync(Guid followerId, Guid followingId)
    {
        if (followerId == followingId)
            throw new InvalidOperationException("You cannot follow yourself");

        var target = await _userRepository.GetByIdAsync(followingId);
        if (target == null)
            throw new EntityNotFoundException("User not found");

        var existing = await _followRepository.GetAsync(followerId, followingId);
        if (existing == null)
        {
            await _followRepository.AddAsync(new Follow { FollowerId = followerId, FollowingId = followingId });
            await NotifyNewFollowerAsync(followerId, target);
        }

        return await _followRepository.GetFollowerCountAsync(followingId);
    }

    private async Task NotifyNewFollowerAsync(Guid followerId, User target)
    {
        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
            return;

        var baseUrl = _configuration["Frontend:BaseUrl"] ?? "";
        var html = $"<p><strong>{follower.GetFullName()}</strong> started following you.</p>"
            + $"<p><a href=\"{baseUrl}/authors/{followerId}\">View their profile</a></p>";

        await _emailService.SendEmailAsync(target.Email, "You have a new follower", html);
    }

    public async Task<int> UnfollowAsync(Guid followerId, Guid followingId)
    {
        var existing = await _followRepository.GetAsync(followerId, followingId);
        if (existing != null)
        {
            await _followRepository.DeleteAsync(existing);
        }

        return await _followRepository.GetFollowerCountAsync(followingId);
    }
}
