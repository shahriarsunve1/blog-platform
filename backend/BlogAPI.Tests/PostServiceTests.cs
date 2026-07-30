using BlogAPI.Core.DTOs;
using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ITagRepository> _tagRepository = new();
    private readonly PostService _sut;

    public PostServiceTests()
    {
        _sut = new PostService(
            _postRepository.Object,
            _userRepository.Object,
            _categoryRepository.Object,
            _tagRepository.Object);

        _categoryRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<Category>());
        _tagRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<Tag>());
    }

    [Fact]
    public async Task CreatePostAsync_UnknownUser_ThrowsEntityNotFound()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.CreatePostAsync(Guid.NewGuid(), new CreatePostDto { Title = "T", Excerpt = "E", Content = "C", Status = "Draft" }));
    }

    [Fact]
    public async Task CreatePostAsync_HonorsRequestedStatus()
    {
        // Regression test: CreatePostAsync used to hardcode every new post to Draft
        // regardless of what the client requested.
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

        var result = await _sut.CreatePostAsync(userId, new CreatePostDto
        {
            Title = "Title",
            Excerpt = "Excerpt",
            Content = "Content",
            Status = "Published"
        });

        Assert.Equal("Published", result.Status);
        Assert.NotNull(result.PublishedAt);
    }

    [Fact]
    public async Task CreatePostAsync_AttachesRequestedCategoriesAndTags()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
        _categoryRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(categoryId))))
            .ReturnsAsync(new List<Category> { new() { Id = categoryId, Name = "Technology", Slug = "technology" } });
        _tagRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(tagId))))
            .ReturnsAsync(new List<Tag> { new() { Id = tagId, Name = "Tutorial", Slug = "tutorial" } });
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

        var result = await _sut.CreatePostAsync(userId, new CreatePostDto
        {
            Title = "Title",
            Excerpt = "Excerpt",
            Content = "Content",
            Status = "Draft",
            CategoryIds = new List<Guid> { categoryId },
            TagIds = new List<Guid> { tagId }
        });

        Assert.Contains("Technology", result.Categories);
        Assert.Contains("Tutorial", result.Tags);
    }

    [Fact]
    public async Task GetPostByIdAsync_UnknownPost_ThrowsEntityNotFound()
    {
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Post?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetPostByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdatePostAsync_NotTheAuthor_ThrowsUnauthorized()
    {
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId, Status = PostStatus.Draft };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.UpdatePostAsync(post.Id, new UpdatePostDto { Title = "T", Excerpt = "E", Content = "C", Status = "Draft" }, otherUserId));
    }

    [Fact]
    public async Task UpdatePostAsync_ByAuthor_UpdatesFieldsAndReplacesTaxonomy()
    {
        var authorId = Guid.NewGuid();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = authorId,
            Title = "Old title",
            Status = PostStatus.Draft,
            Categories = new List<Category> { new() { Id = Guid.NewGuid(), Name = "Stale", Slug = "stale" } }
        };
        var newCategoryId = Guid.NewGuid();
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);
        _categoryRepository.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(newCategoryId))))
            .ReturnsAsync(new List<Category> { new() { Id = newCategoryId, Name = "Fresh", Slug = "fresh" } });

        var result = await _sut.UpdatePostAsync(post.Id, new UpdatePostDto
        {
            Title = "New title",
            Excerpt = "E",
            Content = "C",
            Status = "Published",
            CategoryIds = new List<Guid> { newCategoryId }
        }, authorId);

        Assert.Equal("New title", result.Title);
        Assert.Equal("Published", result.Status);
        Assert.Equal(new[] { "Fresh" }, result.Categories);
    }

    [Fact]
    public async Task DeletePostAsync_NotTheAuthor_ThrowsUnauthorized()
    {
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeletePostAsync(post.Id, otherUserId));
    }

    [Fact]
    public async Task DeletePostAsync_ByAuthor_DeletesPost()
    {
        var authorId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = authorId };
        _postRepository.Setup(r => r.GetPostWithDetailsAsync(post.Id)).ReturnsAsync(post);

        await _sut.DeletePostAsync(post.Id, authorId);

        _postRepository.Verify(r => r.DeleteAsync(post), Times.Once);
    }
}
