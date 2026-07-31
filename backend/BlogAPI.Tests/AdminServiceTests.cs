using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class AdminServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<ICommentRepository> _commentRepository = new();
    private readonly Mock<ILikeRepository> _likeRepository = new();
    private readonly Mock<IFollowRepository> _followRepository = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _sut = new AdminService(
            _userRepository.Object,
            _postRepository.Object,
            _commentRepository.Object,
            _likeRepository.Object,
            _followRepository.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_AggregatesCountsFromAllRepositories()
    {
        _userRepository.Setup(r => r.CountAsync()).ReturnsAsync(10);
        _postRepository.Setup(r => r.CountByStatusAsync(PostStatus.Published)).ReturnsAsync(5);
        _postRepository.Setup(r => r.CountByStatusAsync(PostStatus.Draft)).ReturnsAsync(3);
        _postRepository.Setup(r => r.CountByStatusAsync(PostStatus.Archived)).ReturnsAsync(1);
        _commentRepository.Setup(r => r.CountAsync()).ReturnsAsync(20);
        _likeRepository.Setup(r => r.CountAsync()).ReturnsAsync(30);
        _followRepository.Setup(r => r.CountAsync()).ReturnsAsync(7);
        _userRepository.Setup(r => r.GetRecentAsync(5)).ReturnsAsync(new List<User>());
        _postRepository.Setup(r => r.GetRecentAsync(5)).ReturnsAsync(new List<Post>());

        var result = await _sut.GetDashboardAsync();

        Assert.Equal(10, result.TotalUsers);
        Assert.Equal(9, result.TotalPosts);
        Assert.Equal(5, result.PublishedPosts);
        Assert.Equal(3, result.DraftPosts);
        Assert.Equal(1, result.ArchivedPosts);
        Assert.Equal(20, result.TotalComments);
        Assert.Equal(30, result.TotalLikes);
        Assert.Equal(7, result.TotalFollows);
    }

    [Fact]
    public async Task GetDashboardAsync_MapsRecentUsersAndPosts()
    {
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        _userRepository.Setup(r => r.GetRecentAsync(5)).ReturnsAsync(new List<User>
        {
            new() { Id = userId, FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com", Role = UserRole.Admin }
        });
        _postRepository.Setup(r => r.GetRecentAsync(5)).ReturnsAsync(new List<Post>
        {
            new() { Id = postId, Title = "Hello World", Status = PostStatus.Draft, Author = new User { Id = authorId, FirstName = "Grace", LastName = "Hopper" } }
        });

        var result = await _sut.GetDashboardAsync();

        var user = Assert.Single(result.RecentUsers);
        Assert.Equal(userId, user.Id);
        Assert.Equal("Ada Lovelace", user.FullName);
        Assert.Equal("Admin", user.Role);

        var post = Assert.Single(result.RecentPosts);
        Assert.Equal(postId, post.Id);
        Assert.Equal("Hello World", post.Title);
        Assert.Equal("Draft", post.Status);
        Assert.Equal("Grace Hopper", post.AuthorName);
    }
}
