using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepository = new();
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(_likeRepository.Object, _postRepository.Object);
    }

    [Fact]
    public async Task LikeAsync_UnknownPost_ThrowsEntityNotFound()
    {
        _postRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Post?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.LikeAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task LikeAsync_NotYetLiked_AddsLikeAndReturnsCount()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId });
        _likeRepository.Setup(r => r.GetByPostAndUserAsync(postId, userId)).ReturnsAsync((Like?)null);
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(postId))
            .ReturnsAsync(new Post { Id = postId, Likes = new List<Like> { new() { PostId = postId, UserId = userId } } });

        var count = await _sut.LikeAsync(postId, userId);

        _likeRepository.Verify(r => r.AddAsync(It.Is<Like>(l => l.PostId == postId && l.UserId == userId)), Times.Once);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task LikeAsync_AlreadyLiked_IsIdempotent()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingLike = new Like { PostId = postId, UserId = userId };
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId });
        _likeRepository.Setup(r => r.GetByPostAndUserAsync(postId, userId)).ReturnsAsync(existingLike);
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(postId))
            .ReturnsAsync(new Post { Id = postId, Likes = new List<Like> { existingLike } });

        await _sut.LikeAsync(postId, userId);

        _likeRepository.Verify(r => r.AddAsync(It.IsAny<Like>()), Times.Never);
    }

    [Fact]
    public async Task UnlikeAsync_WasLiked_RemovesLikeAndReturnsCount()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingLike = new Like { PostId = postId, UserId = userId };
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId });
        _likeRepository.Setup(r => r.GetByPostAndUserAsync(postId, userId)).ReturnsAsync(existingLike);
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(postId))
            .ReturnsAsync(new Post { Id = postId, Likes = new List<Like>() });

        var count = await _sut.UnlikeAsync(postId, userId);

        _likeRepository.Verify(r => r.DeleteAsync(existingLike), Times.Once);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UnlikeAsync_NotLiked_IsIdempotent()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId });
        _likeRepository.Setup(r => r.GetByPostAndUserAsync(postId, userId)).ReturnsAsync((Like?)null);
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(postId))
            .ReturnsAsync(new Post { Id = postId, Likes = new List<Like>() });

        await _sut.UnlikeAsync(postId, userId);

        _likeRepository.Verify(r => r.DeleteAsync(It.IsAny<Like>()), Times.Never);
    }
}
