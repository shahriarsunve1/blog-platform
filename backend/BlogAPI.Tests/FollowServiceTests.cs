using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class FollowServiceTests
{
    private readonly Mock<IFollowRepository> _followRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly FollowService _sut;

    public FollowServiceTests()
    {
        _sut = new FollowService(_followRepository.Object, _userRepository.Object);
    }

    [Fact]
    public async Task FollowAsync_Self_Throws()
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.FollowAsync(userId, userId));
    }

    [Fact]
    public async Task FollowAsync_UnknownTarget_ThrowsEntityNotFound()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.FollowAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task FollowAsync_NotYetFollowing_AddsFollowAndReturnsCount()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(followingId)).ReturnsAsync(new User { Id = followingId });
        _followRepository.Setup(r => r.GetAsync(followerId, followingId)).ReturnsAsync((Follow?)null);
        _followRepository.Setup(r => r.GetFollowerCountAsync(followingId)).ReturnsAsync(1);

        var count = await _sut.FollowAsync(followerId, followingId);

        _followRepository.Verify(r => r.AddAsync(It.Is<Follow>(f => f.FollowerId == followerId && f.FollowingId == followingId)), Times.Once);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task FollowAsync_AlreadyFollowing_IsIdempotent()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(followingId)).ReturnsAsync(new User { Id = followingId });
        _followRepository.Setup(r => r.GetAsync(followerId, followingId))
            .ReturnsAsync(new Follow { FollowerId = followerId, FollowingId = followingId });
        _followRepository.Setup(r => r.GetFollowerCountAsync(followingId)).ReturnsAsync(1);

        await _sut.FollowAsync(followerId, followingId);

        _followRepository.Verify(r => r.AddAsync(It.IsAny<Follow>()), Times.Never);
    }

    [Fact]
    public async Task UnfollowAsync_WasFollowing_RemovesFollow()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        var existing = new Follow { FollowerId = followerId, FollowingId = followingId };
        _followRepository.Setup(r => r.GetAsync(followerId, followingId)).ReturnsAsync(existing);
        _followRepository.Setup(r => r.GetFollowerCountAsync(followingId)).ReturnsAsync(0);

        var count = await _sut.UnfollowAsync(followerId, followingId);

        _followRepository.Verify(r => r.DeleteAsync(existing), Times.Once);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UnfollowAsync_NotFollowing_IsIdempotent()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        _followRepository.Setup(r => r.GetAsync(followerId, followingId)).ReturnsAsync((Follow?)null);
        _followRepository.Setup(r => r.GetFollowerCountAsync(followingId)).ReturnsAsync(0);

        await _sut.UnfollowAsync(followerId, followingId);

        _followRepository.Verify(r => r.DeleteAsync(It.IsAny<Follow>()), Times.Never);
    }
}
