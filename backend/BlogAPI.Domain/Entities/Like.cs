namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents a user's like on a blog post
/// </summary>
public class Like
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Post? Post { get; set; }
    public User? User { get; set; }
}
