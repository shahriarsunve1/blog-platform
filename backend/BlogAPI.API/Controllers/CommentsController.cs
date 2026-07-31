using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;
using BlogAPI.Core.Services;
using BlogAPI.Core.DTOs;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.API.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IValidator<CreateCommentDto> _createValidator;

    public CommentsController(ICommentService commentService, IValidator<CreateCommentDto> createValidator)
    {
        _commentService = commentService;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Get all comments for a post
    /// </summary>
    [HttpGet("api/posts/{postId}/comments")]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetByPost(Guid postId)
    {
        var result = await _commentService.GetByPostIdAsync(postId);
        return Ok(ApiResponse<List<CommentDto>>.Ok(result));
    }

    /// <summary>
    /// Add a comment to a post (authenticated)
    /// </summary>
    [Authorize]
    [EnableRateLimiting("comments")]
    [HttpPost("api/posts/{postId}/comments")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> Create(Guid postId, CreateCommentDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);

        try
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
            var result = await _commentService.CreateAsync(postId, userId, request);
            return Ok(ApiResponse<CommentDto>.Ok(result, "Comment added", 201));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<CommentDto>.Fail(ex.Message, 404));
        }
    }

    /// <summary>
    /// Delete a comment (author or admin only)
    /// </summary>
    [Authorize]
    [HttpDelete("api/comments/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
            await _commentService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.Ok(new { }, "Comment deleted"));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message, 403));
        }
    }
}
