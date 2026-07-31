using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// Authentication service implementation with JWT and password hashing
/// </summary>
public class AuthServiceImpl : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthServiceImpl(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto request)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Create new user
        var user = new User
        {
            Username = request.Email.Split('@')[0],
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);

        return await IssueTokensAsync(createdUser);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginUserDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive");
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken)
            ?? throw new UnauthorizedException("Invalid access token");

        var userIdClaim = principal.FindFirst("id")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid access token");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (user.RefreshTokenHash == null ||
            user.RefreshTokenExpiresAt == null ||
            user.RefreshTokenExpiresAt < DateTime.UtcNow ||
            user.RefreshTokenHash != HashRefreshToken(request.RefreshToken))
        {
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        return await IssueTokensAsync(user);
    }

    public async Task<string> GenerateAccessTokenAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException("User not found");

        return GenerateAccessToken(user);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        // Refresh tokens are typically stored in DB, but for simplicity generating here
        return GenerateRefreshToken();
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Issues a fresh access/refresh token pair for the user, persisting the new
    /// refresh token hash (rotation - the old refresh token stops working).
    /// </summary>
    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshDays = _configuration.GetValue<int?>("Jwt:RefreshTokenExpirationDays") ?? 7;

        user.RefreshTokenHash = HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
        await _userRepository.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _configuration.GetValue<int>("Jwt:ExpirationMinutes") * 60,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // expired tokens are expected here
            }, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("id", user.Id.ToString()),
                new System.Security.Claims.Claim("email", user.Email),
                new System.Security.Claims.Claim("role", user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }

}
