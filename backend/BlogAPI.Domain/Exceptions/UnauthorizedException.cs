namespace BlogAPI.Domain.Exceptions;

/// <summary>
/// Exception thrown when authorization fails
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
