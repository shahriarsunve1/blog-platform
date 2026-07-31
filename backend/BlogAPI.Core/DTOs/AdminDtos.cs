namespace BlogAPI.Core.DTOs;

/// <summary>
/// Aggregate stats and recent activity for the admin dashboard
/// </summary>
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int ArchivedPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    public int TotalFollows { get; set; }
    public List<AdminUserSummaryDto> RecentUsers { get; set; } = new();
    public List<AdminPostSummaryDto> RecentPosts { get; set; } = new();
}

public class AdminUserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminPostSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
