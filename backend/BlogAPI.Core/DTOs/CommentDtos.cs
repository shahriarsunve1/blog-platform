namespace BlogAPI.Core.DTOs;

/// <summary>
/// DTO for creating a comment
/// </summary>
public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// DTO for comment response
/// </summary>
public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserDto? Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
