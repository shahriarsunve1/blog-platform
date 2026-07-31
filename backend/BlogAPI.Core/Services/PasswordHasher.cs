namespace BlogAPI.Core.Services;

internal static class PasswordHasher
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash predates the BCrypt migration (or is otherwise malformed) - treat as invalid credentials.
            return false;
        }
    }
}
