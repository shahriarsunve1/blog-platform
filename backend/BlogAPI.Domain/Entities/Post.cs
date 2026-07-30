using BlogAPI.Domain.Enums;

namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents a blog post
/// </summary>
public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public int ViewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public User? Author { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public bool IsPublished() => Status == PostStatus.Published && PublishedAt.HasValue;
}
