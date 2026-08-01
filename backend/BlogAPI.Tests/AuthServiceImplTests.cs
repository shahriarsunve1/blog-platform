using BlogAPI.Core.DTOs;
using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class AuthServiceImplTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly AuthServiceImpl _sut;

    public AuthServiceImplTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-long-enough-for-hmacsha256!!",
                ["Jwt:Issuer"] = "BlogAPI.Tests",
                ["Jwt:Audience"] = "BlogAPI.Tests",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        _sut = new AuthServiceImpl(_userRepository.Object, configuration, _emailService.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUnverifiedUserAndSendsVerificationEmail()
    {
        User? capturedUser = null;
        _userRepository.Setup(r => r.EmailExistsAsync("new@example.com")).ReturnsAsync(false);
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { capturedUser = u; u.Id = Guid.NewGuid(); return u; });

        var result = await _sut.RegisterAsync(new RegisterUserDto
        {
            Email = "new@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User"
        });

        Assert.NotEmpty(result.Message);
        Assert.NotNull(capturedUser);
        Assert.False(capturedUser!.EmailVerified);
        Assert.NotNull(capturedUser.EmailVerificationTokenHash);
        Assert.NotNull(capturedUser.EmailVerificationTokenExpiresAt);
        _emailService.Verify(e => e.SendEmailAsync("new@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_StoresHashedPasswordNotPlainText()
    {
        User? capturedUser = null;
        _userRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { capturedUser = u; u.Id = Guid.NewGuid(); return u; });

        await _sut.RegisterAsync(new RegisterUserDto
        {
            Email = "new@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User"
        });

        Assert.NotNull(capturedUser);
        Assert.NotEqual("password123", capturedUser!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", capturedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_Throws()
    {
        _userRepository.Setup(r => r.EmailExistsAsync("taken@example.com")).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(new RegisterUserDto
        {
            Email = "taken@example.com",
            Password = "password123",
            FirstName = "A",
            LastName = "B"
        }));
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true,
            Role = UserRole.User
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "password123" });

        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "wrong-password" }));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginUserDto { Email = "nobody@example.com", Password = "password123" }));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = false
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "password123" }));
    }

    [Fact]
    public async Task LoginAsync_UnverifiedEmail_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "unverified@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true,
            EmailVerified = false
        };
        _userRepository.Setup(r => r.GetByEmailAsync("unverified@example.com")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginUserDto { Email = "unverified@example.com", Password = "password123" }));
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_MarksUserVerifiedAndClearsToken()
    {
        User? capturedUser = null;
        _userRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { capturedUser = u; u.Id = Guid.NewGuid(); return u; });

        string? sentHtml = null;
        _emailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => sentHtml = html)
            .Returns(Task.CompletedTask);

        await _sut.RegisterAsync(new RegisterUserDto
        {
            Email = "new@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User"
        });

        var token = ExtractTokenFromEmailLink(sentHtml!);
        _userRepository.Setup(r => r.GetByEmailVerificationTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(capturedUser);

        await _sut.VerifyEmailAsync(token);

        Assert.True(capturedUser!.EmailVerified);
        Assert.Null(capturedUser.EmailVerificationTokenHash);
        Assert.Null(capturedUser.EmailVerificationTokenExpiresAt);
    }

    [Fact]
    public async Task VerifyEmailAsync_UnknownToken_ThrowsUnauthorized()
    {
        _userRepository.Setup(r => r.GetByEmailVerificationTokenHashAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.VerifyEmailAsync("bogus-token"));
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredToken_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            EmailVerified = false,
            EmailVerificationTokenHash = "some-hash",
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        _userRepository.Setup(r => r.GetByEmailVerificationTokenHashAsync(It.IsAny<string>())).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.VerifyEmailAsync("expired-token"));
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_AlreadyVerifiedUser_DoesNotSendEmail()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", EmailVerified = true };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        await _sut.ResendVerificationEmailAsync("user@example.com");

        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UnknownEmail_DoesNotThrowOrSendEmail()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await _sut.ResendVerificationEmailAsync("nobody@example.com");

        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UnverifiedUser_SendsNewVerificationEmail()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", EmailVerified = false };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        await _sut.ResendVerificationEmailAsync("user@example.com");

        _emailService.Verify(e => e.SendEmailAsync("user@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static string ExtractTokenFromEmailLink(string html)
    {
        var marker = "token=";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = html.IndexOf('"', start);
        return Uri.UnescapeDataString(html[start..end]);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidPair_IssuesNewTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        // IssueTokensAsync mutates `user` in place, so this returns the same
        // instance with whatever refresh-token hash was just stored on login.
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var loginResult = await _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "password123" });

        var refreshResult = await _sut.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            AccessToken = loginResult.AccessToken,
            RefreshToken = loginResult.RefreshToken
        });

        Assert.NotEmpty(refreshResult.AccessToken);
        Assert.NotEmpty(refreshResult.RefreshToken);
        Assert.NotEqual(loginResult.RefreshToken, refreshResult.RefreshToken); // rotated
    }

    [Fact]
    public async Task RefreshTokenAsync_WrongRefreshToken_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var loginResult = await _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "password123" });

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            AccessToken = loginResult.AccessToken,
            RefreshToken = "not-the-real-refresh-token"
        }));
    }

    [Fact]
    public async Task RefreshTokenAsync_AfterRotation_OldRefreshTokenNoLongerWorks()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var loginResult = await _sut.LoginAsync(new LoginUserDto { Email = "user@example.com", Password = "password123" });
        await _sut.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            AccessToken = loginResult.AccessToken,
            RefreshToken = loginResult.RefreshToken
        });

        // The original refresh token was rotated out - reusing it must fail.
        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            AccessToken = loginResult.AccessToken,
            RefreshToken = loginResult.RefreshToken
        }));
    }

    [Fact]
    public async Task LoginAsync_LegacyPreBCryptHash_FailsCleanlyInsteadOfThrowing()
    {
        // Regression test: accounts created before the BCrypt migration have a raw
        // SHA256-based hash that isn't valid BCrypt input. Verifying against it must
        // report "invalid credentials", not blow up with an unhandled exception.
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "legacy@example.com",
            PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes("password123"))),
            IsActive = true
        };
        _userRepository.Setup(r => r.GetByEmailAsync("legacy@example.com")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginUserDto { Email = "legacy@example.com", Password = "password123" }));
    }
}
