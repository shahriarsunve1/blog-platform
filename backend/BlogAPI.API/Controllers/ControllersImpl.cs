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
    private readonly ILikeService _likeService;
    private readonly IValidator<CreatePostDto> _createValidator;
    private readonly IValidator<UpdatePostDto> _updateValidator;

    public PostsController(
        IPostService postService,
        ILikeService likeService,
        IValidator<CreatePostDto> createValidator,
        IValidator<UpdatePostDto> updateValidator)
    {
        _postService = postService;
        _likeService = likeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// The requesting user's id if authenticated, otherwise null. Works on
    /// endpoints without [Authorize] too - the JWT middleware still populates
    /// claims for any request that presents a valid token.
    /// </summary>
    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirst("id")?.Value, out var id) ? id : null;

    /// <summary>
    /// Get published posts with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PostDto>>>> GetPublishedPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? tagId = null,
        [FromQuery] string? search = null,
        [FromQuery] Guid? authorId = null)
    {
        var result = await _postService.GetPublishedPostsAsync(pageNumber, pageSize, categoryId, tagId, search, CurrentUserId, authorId);
        return Ok(ApiResponse<PaginatedResponse<PostDto>>.Ok(result));
    }

    /// <summary>
    /// Get all of the current user's own posts, any status (draft/published/archived)
    /// </summary>
    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetMyPosts()
    {
        var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
        var result = await _postService.GetUserPostsAsync(userId);
        return Ok(ApiResponse<List<PostDto>>.Ok(result));
    }

    /// <summary>
    /// Top posts ranked by a time-decayed engagement score
    /// </summary>
    [HttpGet("trending")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetTrending([FromQuery] int count = 6)
    {
        var result = await _postService.GetTrendingAsync(count, CurrentUserId);
        return Ok(ApiResponse<List<PostDto>>.Ok(result));
    }

    /// <summary>
    /// Posts picked for the current user based on followed authors and liked categories,
    /// falling back to trending posts for anonymous users or when there isn't enough signal
    /// </summary>
    [HttpGet("suggested")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetSuggested([FromQuery] int count = 6)
    {
        var result = await _postService.GetSuggestedAsync(count, CurrentUserId);
        return Ok(ApiResponse<List<PostDto>>.Ok(result));
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPostById(Guid id)
    {
        try
        {
            var result = await _postService.GetPostByIdAsync(id, CurrentUserId);
            return Ok(ApiResponse<PostDto>.Ok(result));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<PostDto>.Fail(ex.Message, 404));
        }
    }

    /// <summary>
    /// Like a post (idempotent)
    /// </summary>
    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<ActionResult<ApiResponse<int>>> Like(Guid id)
    {
        try
        {
            var likeCount = await _likeService.LikeAsync(id, CurrentUserId!.Value);
            return Ok(ApiResponse<int>.Ok(likeCount, "Post liked"));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<int>.Fail(ex.Message, 404));
        }
    }

    /// <summary>
    /// Unlike a post (idempotent)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}/like")]
    public async Task<ActionResult<ApiResponse<int>>> Unlike(Guid id)
    {
        try
        {
            var likeCount = await _likeService.UnlikeAsync(id, CurrentUserId!.Value);
            return Ok(ApiResponse<int>.Ok(likeCount, "Post unliked"));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<int>.Fail(ex.Message, 404));
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
    private readonly IFollowService _followService;

    public UsersController(IUserService userService, IFollowService followService)
    {
        _userService = userService;
        _followService = followService;
    }

    /// <summary>
    /// The requesting user's id if authenticated, otherwise null.
    /// </summary>
    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirst("id")?.Value, out var id) ? id : null;

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id, CurrentUserId);
            return Ok(ApiResponse<UserDto>.Ok(result));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message, 404));
        }
    }

    /// <summary>
    /// Follow a user (idempotent)
    /// </summary>
    [Authorize]
    [HttpPost("{id}/follow")]
    public async Task<ActionResult<ApiResponse<int>>> Follow(Guid id)
    {
        try
        {
            var followerCount = await _followService.FollowAsync(CurrentUserId!.Value, id);
            return Ok(ApiResponse<int>.Ok(followerCount, "Followed"));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ApiResponse<int>.Fail(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<int>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Unfollow a user (idempotent)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}/follow")]
    public async Task<ActionResult<ApiResponse<int>>> Unfollow(Guid id)
    {
        var followerCount = await _followService.UnfollowAsync(CurrentUserId!.Value, id);
        return Ok(ApiResponse<int>.Ok(followerCount, "Unfollowed"));
    }
}

/// <summary>
/// Admin-only endpoints. The JWT's "role" claim is one of the JwtSecurityTokenHandler's
/// default inbound-mapped short claim names, so it arrives on HttpContext.User as
/// ClaimTypes.Role - [Authorize(Roles=...)] picks it up with no extra configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Platform-wide stats and recent activity (admin only)
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }
}
