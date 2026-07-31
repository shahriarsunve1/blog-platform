using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// Follow service implementation. Following/unfollowing is idempotent, same
/// as likes. Following yourself is rejected.
/// </summary>
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly IUserRepository _userRepository;

    public FollowService(IFollowRepository followRepository, IUserRepository userRepository)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
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
        }

        return await _followRepository.GetFollowerCountAsync(followingId);
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
