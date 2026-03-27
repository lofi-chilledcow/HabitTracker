using AuthService.Commands;
using Xunit;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly JwtTokenService _jwt;

    private static readonly Role DefaultRole = new()
    {
        Id = Guid.NewGuid(),
        Name = "User",
        Description = "Default role"
    };

    public LoginCommandHandlerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-that-is-long-enough-for-hmac",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Jwt:RefreshTokenExpiryDays"] = "7"
            })
            .Build();

        _jwt = new JwtTokenService(config);
    }

    private LoginCommandHandler CreateHandler() =>
        new(_users.Object, _refreshTokens.Object, _jwt);

    private static User UserWithPassword(string password) => new()
    {
        Id = Guid.NewGuid(),
        Username = "alice",
        Email = "alice@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        RoleId = DefaultRole.Id,
        Role = DefaultRole
    };

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAccessTokenAndRefreshToken()
    {
        var user = UserWithPassword("Password1!");

        _users.Setup(r => r.GetByEmailWithRoleAsync("alice@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _refreshTokens.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((RefreshToken t, CancellationToken _) => t);

        var result = await CreateHandler().Handle(
            new LoginCommand("alice@example.com", "Password1!"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("alice", result.Username);
        Assert.Equal("alice@example.com", result.Email);

        _refreshTokens.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = UserWithPassword("Password1!");

        _users.Setup(r => r.GetByEmailWithRoleAsync("alice@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(
                new LoginCommand("alice@example.com", "WrongPassword!"),
                CancellationToken.None));

        Assert.Equal("Invalid email or password.", ex.Message);
        _refreshTokens.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _users.Setup(r => r.GetByEmailWithRoleAsync("nobody@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(
                new LoginCommand("nobody@example.com", "Password1!"),
                CancellationToken.None));

        Assert.Equal("Invalid email or password.", ex.Message);
        _refreshTokens.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
