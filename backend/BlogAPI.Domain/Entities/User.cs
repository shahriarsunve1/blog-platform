using BlogAPI.Domain.Enums;

namespace BlogAPI.Domain.Entities;

/// <summary>
/// Represents a user in the blog platform
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public bool EmailOnComment { get; set; } = true;
    public bool EmailOnFollow { get; set; } = true;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    // Defaults to true so existing/seed users aren't locked out - registration
    // explicitly sets this false and only flips it once the link is clicked.
    public bool EmailVerified { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
