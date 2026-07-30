namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents a comment on a blog post
/// </summary>
public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Post? Post { get; set; }
    public User? Author { get; set; }
}
