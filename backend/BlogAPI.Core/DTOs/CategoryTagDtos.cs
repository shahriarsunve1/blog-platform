namespace BlogAPI.Core.DTOs;

/// <summary>
/// DTO for category response
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a category
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO for tag response
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a tag
/// </summary>
public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
}
