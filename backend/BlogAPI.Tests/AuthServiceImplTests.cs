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

        _sut = new AuthServiceImpl(_userRepository.Object, configuration);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        _userRepository.Setup(r => r.EmailExistsAsync("new@example.com")).ReturnsAsync(false);
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = Guid.NewGuid(); return u; });

        var result = await _sut.RegisterAsync(new RegisterUserDto
        {
            Email = "new@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User"
        });

        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("new@example.com", result.User.Email);
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
