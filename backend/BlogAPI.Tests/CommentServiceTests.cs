using BlogAPI.Core.DTOs;
using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository = new();
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_commentRepository.Object, _postRepository.Object, _userRepository.Object, _emailService.Object, _configuration.Object);
    }

    [Fact]
    public async Task CreateAsync_UnknownPost_ThrowsEntityNotFound()
    {
        _postRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Post?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateCommentDto { Content = "Nice post!" }));
    }

    [Fact]
    public async Task CreateAsync_ValidPostAndUser_ReturnsCommentWithAuthor()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId, UserId = userId });
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, FirstName = "A", LastName = "B" });
        _commentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);

        var result = await _sut.CreateAsync(postId, userId, new CreateCommentDto { Content = "Nice post!" });

        Assert.Equal("Nice post!", result.Content);
        Assert.Equal("A", result.Author?.FirstName);
    }

    [Fact]
    public async Task CreateAsync_CommentOnOwnPost_DoesNotSendEmail()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId, UserId = userId });
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, FirstName = "A", LastName = "B" });
        _commentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);

        await _sut.CreateAsync(postId, userId, new CreateCommentDto { Content = "Nice post!" });

        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CommentOnSomeoneElsesPost_NotifiesAuthor()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var commenterId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId, UserId = authorId, Title = "Hello" });
        _userRepository.Setup(r => r.GetByIdAsync(commenterId)).ReturnsAsync(new User { Id = commenterId, FirstName = "Commenter", LastName = "X" });
        _userRepository.Setup(r => r.GetByIdAsync(authorId)).ReturnsAsync(new User { Id = authorId, Email = "author@example.com" });
        _commentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);

        await _sut.CreateAsync(postId, commenterId, new CreateCommentDto { Content = "Nice post!" });

        _emailService.Verify(e => e.SendEmailAsync("author@example.com", It.Is<string>(s => s.Contains("Hello")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnknownComment_ThrowsEntityNotFound()
    {
        _commentRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Comment?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_ByCommentAuthor_Succeeds()
    {
        var authorId = Guid.NewGuid();
        var comment = new Comment { Id = Guid.NewGuid(), UserId = authorId };
        _commentRepository.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        await _sut.DeleteAsync(comment.Id, authorId);

        _commentRepository.Verify(r => r.DeleteAsync(comment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByRandomUser_ThrowsUnauthorized()
    {
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var comment = new Comment { Id = Guid.NewGuid(), UserId = authorId };
        _commentRepository.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);
        _userRepository.Setup(r => r.GetByIdAsync(otherUserId)).ReturnsAsync(new User { Id = otherUserId, Role = UserRole.User });

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeleteAsync(comment.Id, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_ByAdmin_Succeeds()
    {
        var authorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var comment = new Comment { Id = Guid.NewGuid(), UserId = authorId };
        _commentRepository.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);
        _userRepository.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(new User { Id = adminId, Role = UserRole.Admin });

        await _sut.DeleteAsync(comment.Id, adminId);

        _commentRepository.Verify(r => r.DeleteAsync(comment), Times.Once);
    }
}
