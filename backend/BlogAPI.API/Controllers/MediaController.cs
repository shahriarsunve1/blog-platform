using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using BlogAPI.Core.Services;
using BlogAPI.Core.DTOs;

namespace BlogAPI.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// Upload an image to embed in post content (authenticated)
    /// </summary>
    [Authorize]
    [EnableRateLimiting("media")]
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MediaFileDto>>> Upload(IFormFile file)
    {
        if (file == null)
            return BadRequest(ApiResponse<MediaFileDto>.Fail("No file uploaded", 400));

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var userId = Guid.Parse(User.FindFirst("id")?.Value ?? Guid.Empty.ToString());
        var mediaFile = await _mediaService.UploadAsync(userId, file.FileName, file.ContentType, stream.ToArray());

        var url = $"{Request.Scheme}://{Request.Host}/api/media/{mediaFile.Id}";
        var dto = new MediaFileDto { Id = mediaFile.Id, Url = url };

        return Ok(ApiResponse<MediaFileDto>.Ok(dto, "File uploaded", 201));
    }

    /// <summary>
    /// Fetch a previously uploaded image (public, so it can be rendered in published posts)
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(Guid id)
    {
        var mediaFile = await _mediaService.GetAsync(id);
        if (mediaFile == null)
            return NotFound();

        return File(mediaFile.Data, mediaFile.ContentType);
    }
}
