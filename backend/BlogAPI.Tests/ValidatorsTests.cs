using BlogAPI.Core.DTOs;
using BlogAPI.Core.Validators;
using Xunit;

namespace BlogAPI.Tests;

public class ValidatorsTests
{
    [Theory]
    [InlineData("not-an-email", "password123", "First", "Last", false)]
    [InlineData("valid@example.com", "short", "First", "Last", false)]
    [InlineData("valid@example.com", "password123", "", "Last", false)]
    [InlineData("valid@example.com", "password123", "First", "Last", true)]
    public void RegisterUserDtoValidator_ValidatesExpectedRules(
        string email, string password, string firstName, string lastName, bool expectedValid)
    {
        var validator = new RegisterUserDtoValidator();
        var result = validator.Validate(new RegisterUserDto
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        });

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("Title", "Short excerpt long enough", "Content that is definitely at least fifty characters long for validation.", "Draft", true)]
    [InlineData("Ti", "Short excerpt long enough", "Content that is definitely at least fifty characters long for validation.", "Draft", false)] // title too short
    [InlineData("Title", "Short excerpt long enough", "Too short", "Draft", false)] // content too short
    [InlineData("Title", "Short excerpt long enough", "Content that is definitely at least fifty characters long for validation.", "NotAStatus", false)] // invalid status
    public void CreatePostDtoValidator_ValidatesExpectedRules(
        string title, string excerpt, string content, string status, bool expectedValid)
    {
        var validator = new CreatePostDtoValidator();
        var result = validator.Validate(new CreatePostDto
        {
            Title = title,
            Excerpt = excerpt,
            Content = content,
            Status = status
        });

        Assert.Equal(expectedValid, result.IsValid);
    }
}
