using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BlogAPI.Core.Services;
using BlogAPI.Core.DTOs;
using BlogAPI.Domain.Exceptions;
using FluentValidation;

namespace BlogAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterUserDto> registerValidator,
        IValidator<LoginUserDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterUserDto request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginUserDto request)
    {
        await _loginValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful"));
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message, 401));
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(RefreshTokenRequestDto request)
    {
        await _refreshValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed"));
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message, 401));
        }
    }

    /// <summary>
    /// Logout user (clear token on client)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public ActionResult<ApiResponse<object>> Logout()
    {
        return Ok(ApiResponse<object>.Ok(new { }, "Logout successful"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IValidator<CreatePostDto> _createValidator;
    private readonly IValidator<UpdatePostDto> _updateValidator;

    public PostsController(
        IPostService postService,
        IValidator<CreatePostDto> createValidator,
        IValidator<UpdatePostDto> updateValidator)
    {
        _postService = postService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get published posts with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PostDto>>>> GetPublishedPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _postService.GetPublishedPostsAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PaginatedResponse<PostDto>>.Ok(result));
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPostById(Guid id)
    {
        try
        {
            var result = await _postService.GetPostByIdAsync(id);
            return Ok(ApiResponse<PostDto>.Ok(result));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<PostDto>.Fail(ex.Message, 404));
        }
    }

    /// <summary>
    /// Create new post (authenticated)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostDto>>> CreatePost(CreatePostDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);

        try
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
            var result = await _postService.CreatePostAsync(userId, request);
            return CreatedAtAction(nameof(GetPostById), new { id = result.Id }, ApiResponse<PostDto>.Ok(result, "Post created successfully", 201));
        }
        catch (EntityNotFoundException ex)
        {
            return BadRequest(ApiResponse<PostDto>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Update post (author/admin only)
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> UpdatePost(Guid id, UpdatePostDto request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);

        try
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
            var result = await _postService.UpdatePostAsync(id, request, userId);
            return Ok(ApiResponse<PostDto>.Ok(result, "Post updated successfully"));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<PostDto>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedException ex)
        {
            return StatusCode(403, ApiResponse<PostDto>.Fail(ex.Message, 403));
        }
    }

    /// <summary>
    /// Delete post (author/admin only)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePost(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
            await _postService.DeletePostAsync(id, userId);
            return Ok(ApiResponse<object>.Ok(new { }, "Post deleted successfully"));
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

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDto>.Ok(result));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message, 404));
        }
    }
}
