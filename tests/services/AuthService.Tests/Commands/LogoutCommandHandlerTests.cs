using AuthService.Commands;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AuthService.Tests.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly JwtTokenService _jwt;

    public LogoutCommandHandlerTests()
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

    [Fact]
    public async Task Handle_RefreshToken_RevokesStoredTokenHash()
    {
        const string rawToken = "raw-refresh-token";
        var expectedHash = _jwt.HashRefreshToken(rawToken);
        var handler = new LogoutCommandHandler(_refreshTokens.Object, _jwt);

        await handler.Handle(new LogoutCommand(rawToken), CancellationToken.None);

        _refreshTokens.Verify(
            r => r.RevokeByTokenHashAsync(expectedHash, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
