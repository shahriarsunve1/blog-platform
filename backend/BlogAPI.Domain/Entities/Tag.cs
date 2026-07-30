namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents a blog post tag
/// </summary>
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
