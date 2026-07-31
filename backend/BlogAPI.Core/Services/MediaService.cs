using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Core.Services;

/// <summary>
/// Media service implementation. Images are stored as bytes in the database rather than
/// on local disk, since the API runs on a host with an ephemeral filesystem.
/// </summary>
public class MediaService : IMediaService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IGenericRepository<MediaFile> _mediaRepository;

    public MediaService(IGenericRepository<MediaFile> mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<MediaFile> UploadAsync(Guid userId, string fileName, string contentType, byte[] data)
    {
        if (data.Length == 0)
            throw new ArgumentException("File is empty");

        if (data.Length > MaxFileSizeBytes)
            throw new ArgumentException("File exceeds the 5MB size limit");

        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException("Only PNG, JPEG, GIF, and WEBP images are supported");

        var mediaFile = new MediaFile
        {
            UploadedByUserId = userId,
            FileName = fileName,
            ContentType = contentType,
            Data = data,
            CreatedAt = DateTime.UtcNow
        };

        return await _mediaRepository.AddAsync(mediaFile);
    }

    public async Task<MediaFile?> GetAsync(Guid id)
    {
        return await _mediaRepository.GetByIdAsync(id);
    }
}
