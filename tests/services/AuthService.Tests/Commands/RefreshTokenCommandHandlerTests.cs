using AuthService.Commands;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AuthService.Tests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly JwtTokenService _jwt;

    public RefreshTokenCommandHandlerTests()
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

    private RefreshTokenCommandHandler CreateHandler() => new(_refreshTokens.Object, _jwt);

    private static RefreshToken ActiveToken(string tokenHash) => new()
    {
        Id = Guid.NewGuid(),
        TokenHash = tokenHash,
        UserId = Guid.NewGuid(),
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        User = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            IsActive = true,
            Role = new Role { Id = Guid.NewGuid(), Name = "User" }
        }
    };

    [Fact]
    public async Task Handle_ActiveToken_RotatesRefreshToken()
    {
        const string rawToken = "raw-refresh-token";
        var tokenHash = _jwt.HashRefreshToken(rawToken);
        var existing = ActiveToken(tokenHash);
        existing.UserId = existing.User.Id;

        _refreshTokens.Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existing);
        _refreshTokens.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((RefreshToken t, CancellationToken _) => t);

        var result = await CreateHandler().Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(rawToken, result.RefreshToken);
        _refreshTokens.Verify(r => r.RevokeAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(
            r => r.CreateAsync(
                It.Is<RefreshToken>(t => t.UserId == existing.UserId && !string.IsNullOrWhiteSpace(t.TokenHash)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedAccessException()
    {
        const string rawToken = "raw-refresh-token";
        var tokenHash = _jwt.HashRefreshToken(rawToken);
        var existing = ActiveToken(tokenHash);
        existing.RevokedAt = DateTime.UtcNow;

        _refreshTokens.Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existing);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new RefreshTokenCommand(rawToken), CancellationToken.None));

        _refreshTokens.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
