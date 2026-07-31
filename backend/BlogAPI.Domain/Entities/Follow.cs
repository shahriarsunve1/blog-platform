namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents one user following another
/// </summary>
public class Follow
{
    public Guid Id { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? FollowerUser { get; set; }
    public User? FollowingUser { get; set; }
}
