using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace BlogAPI.Core.Services;

/// <summary>
/// Comment service implementation
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<List<CommentDto>> GetByPostIdAsync(Guid postId)
    {
        var comments = await _commentRepository.GetByPostIdAsync(postId);
        return comments.Select(MapToDto).ToList();
    }

    public async Task<CommentDto> CreateAsync(Guid postId, Guid userId, CreateCommentDto request)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _commentRepository.AddAsync(comment);
        created.Author = user;

        await NotifyPostAuthorAsync(post, user, request.Content);

        return MapToDto(created);
    }

    private async Task NotifyPostAuthorAsync(Post post, User commenter, string content)
    {
        if (post.UserId == commenter.Id)
            return;

        var author = await _userRepository.GetByIdAsync(post.UserId);
        if (author == null || !author.EmailOnComment)
            return;

        var baseUrl = _configuration["Frontend:BaseUrl"] ?? "";
        var html = $"<p><strong>{commenter.GetFullName()}</strong> commented on your post \"<strong>{post.Title}</strong>\":</p>"
            + $"<blockquote>{content}</blockquote>"
            + $"<p><a href=\"{baseUrl}/posts/{post.Id}\">View the post</a></p>";

        await _emailService.SendEmailAsync(author.Email, $"New comment on \"{post.Title}\"", html);
    }

    public async Task DeleteAsync(Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new EntityNotFoundException("Comment not found");

        if (comment.UserId != userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != UserRole.Admin)
                throw new UnauthorizedException("You do not have permission to delete this comment");
        }

        await _commentRepository.DeleteAsync(comment);
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Author = comment.Author != null ? new UserDto
            {
                Id = comment.Author.Id,
                Email = comment.Author.Email,
                FirstName = comment.Author.FirstName,
                LastName = comment.Author.LastName
            } : null
        };
    }
}
