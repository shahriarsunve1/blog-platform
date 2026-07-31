namespace BlogAPI.Core.DTOs;

/// <summary>
/// DTO for updating the current user's basic profile info
/// </summary>
public class UpdateProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
}

/// <summary>
/// DTO for changing the current user's password
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating the current user's notification preferences
/// </summary>
public class UpdatePreferencesDto
{
    public bool EmailOnComment { get; set; }
    public bool EmailOnFollow { get; set; }
}
