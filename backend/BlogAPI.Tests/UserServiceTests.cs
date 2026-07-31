using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IFollowRepository> _followRepository = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository.Object, _followRepository.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_UnknownUser_ThrowsEntityNotFound()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetUserByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetUserByIdAsync_ComputesFollowerCountAndFollowStatus()
    {
        var userId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _followRepository.Setup(r => r.GetFollowerCountAsync(userId)).ReturnsAsync(3);
        _followRepository.Setup(r => r.GetAsync(viewerId, userId))
            .ReturnsAsync(new Follow { FollowerId = viewerId, FollowingId = userId });

        var followedByViewer = await _sut.GetUserByIdAsync(userId, viewerId);
        var viewedAnonymously = await _sut.GetUserByIdAsync(userId, null);

        Assert.Equal(3, followedByViewer.FollowerCount);
        Assert.True(followedByViewer.IsFollowedByCurrentUser);
        Assert.Equal(3, viewedAnonymously.FollowerCount);
        Assert.False(viewedAnonymously.IsFollowedByCurrentUser);
    }
}
