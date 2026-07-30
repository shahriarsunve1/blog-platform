using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// Comment service implementation
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
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
        return MapToDto(created);
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
