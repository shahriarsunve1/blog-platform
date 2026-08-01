using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
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
    private readonly IValidator<VerifyEmailDto> _verifyEmailValidator;
    private readonly IValidator<ResendVerificationDto> _resendVerificationValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterUserDto> registerValidator,
        IValidator<LoginUserDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshValidator,
        IValidator<VerifyEmailDto> verifyEmailValidator,
        IValidator<ResendVerificationDto> resendVerificationValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _resendVerificationValidator = resendVerificationValidator;
    }

    /// <summary>
    /// Register a new user (inactive until they verify their email)
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResultDto>>> Register(RegisterUserDto request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<RegisterResultDto>.Ok(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RegisterResultDto>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Confirm an emailed verification link
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(VerifyEmailDto request)
    {
        await _verifyEmailValidator.ValidateAndThrowAsync(request);

        try
        {
            await _authService.VerifyEmailAsync(request.Token);
            return Ok(ApiResponse<object>.Ok(new { }, "Email verified successfully"));
        }
        catch (UnauthorizedException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Request a new verification email (e.g. the old link expired)
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("resend-verification")]
    public async Task<ActionResult<ApiResponse<object>>> ResendVerification(ResendVerificationDto request)
    {
        await _resendVerificationValidator.ValidateAndThrowAsync(request);

        await _authService.ResendVerificationEmailAsync(request.Email);
        return Ok(ApiResponse<object>.Ok(new { }, "If an account with that email exists, a verification link has been sent."));
    }

    /// <summary>
    /// Login user
    /// </summary>
    [EnableRateLimiting("auth")]
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
    private readonly IConfiguration _configuration;

    public PostsController(
        IPostService postService,
        ILikeService likeService,
        IValidator<CreatePostDto> createValidator,
        IValidator<UpdatePostDto> updateValidator,
        IConfiguration configuration)
    {
        _postService = postService;
        _likeService = likeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _configuration = configuration;
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
    /// RSS 2.0 feed of the most recent published posts
    /// </summary>
    [HttpGet("feed.xml")]
    public async Task<IActionResult> Feed()
    {
        var result = await _postService.GetPublishedPostsAsync(1, 20);
        var baseUrl = (_configuration["Frontend:BaseUrl"] ?? "").TrimEnd('/');

        var channel = new XElement("channel",
            new XElement("title", "Resonate"),
            new XElement("link", baseUrl),
            new XElement("description", "Recent posts from Resonate"),
            new XElement("language", "en-us"),
            result.Items.Select(post =>
            {
                var postUrl = $"{baseUrl}/posts/{post.Id}";
                return new XElement("item",
                    new XElement("title", post.Title),
                    new XElement("link", postUrl),
                    new XElement("guid", new XAttribute("isPermaLink", "true"), postUrl),
                    new XElement("description", post.Excerpt),
                    new XElement("pubDate", (post.PublishedAt ?? post.CreatedAt).ToString("R")),
                    post.Author != null ? new XElement("author", $"{post.Author.FirstName} {post.Author.LastName}".Trim()) : null
                );
            }));

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss", new XAttribute("version", "2.0"), channel));

        return Content(doc.Declaration + Environment.NewLine + doc.ToString(), "application/rss+xml", Encoding.UTF8);
    }

    /// <summary>
    /// XML sitemap of every published post, for search engine discovery
    /// </summary>
    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var result = await _postService.GetPublishedPostsAsync(1, 5000);
        var baseUrl = (_configuration["Frontend:BaseUrl"] ?? "").TrimEnd('/');

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var homeUrl = new XElement(ns + "url",
            new XElement(ns + "loc", $"{baseUrl}/posts"),
            new XElement(ns + "changefreq", "daily"));

        var postUrls = result.Items.Select(post => new XElement(ns + "url",
            new XElement(ns + "loc", $"{baseUrl}/posts/{post.Id}"),
            new XElement(ns + "lastmod", post.UpdatedAt.ToString("yyyy-MM-dd")),
            new XElement(ns + "changefreq", "weekly")));

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", new[] { homeUrl }.Concat(postUrls)));

        return Content(doc.Declaration + Environment.NewLine + doc.ToString(), "application/xml", Encoding.UTF8);
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
    private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

    public UsersController(
        IUserService userService,
        IFollowService followService,
        IValidator<UpdateProfileDto> updateProfileValidator,
        IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _userService = userService;
        _followService = followService;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
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

    /// <summary>
    /// Update the current user's basic profile info
    /// </summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(UpdateProfileDto request)
    {
        await _updateProfileValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _userService.UpdateProfileAsync(CurrentUserId!.Value, request);
            return Ok(ApiResponse<UserDto>.Ok(result, "Profile updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message, 400));
        }
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    [Authorize]
    [HttpPut("me/password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordDto request)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(request);

        try
        {
            await _userService.ChangePasswordAsync(CurrentUserId!.Value, request);
            return Ok(ApiResponse<object>.Ok(new { }, "Password updated"));
        }
        catch (UnauthorizedException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message, 403));
        }
    }

    /// <summary>
    /// Update the current user's notification preferences
    /// </summary>
    [Authorize]
    [HttpPut("me/preferences")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdatePreferences(UpdatePreferencesDto request)
    {
        var result = await _userService.UpdatePreferencesAsync(CurrentUserId!.Value, request);
        return Ok(ApiResponse<UserDto>.Ok(result, "Preferences updated"));
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
