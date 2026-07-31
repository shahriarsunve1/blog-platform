namespace BlogAPI.Core.DTOs;

/// <summary>
/// A newly uploaded media file, with the URL to fetch it back.
/// </summary>
public class MediaFileDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}
