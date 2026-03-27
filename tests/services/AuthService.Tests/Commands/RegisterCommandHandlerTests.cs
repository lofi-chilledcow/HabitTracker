using AuthService.Commands;
using Xunit;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.Tests.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRoleRepository> _roles = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly JwtTokenService _jwt;

    private static readonly Role DefaultRole = new()
    {
        Id = Guid.NewGuid(),
        Name = "User",
        Description = "Default role"
    };

    public RegisterCommandHandlerTests()
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

    private RegisterCommandHandler CreateHandler() =>
        new(_users.Object, _roles.Object, _refreshTokens.Object, _jwt);

    [Fact]
    public async Task Handle_NewUser_CreatesUserAndReturnsTokens()
    {
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _roles.Setup(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()))
              .ReturnsAsync(DefaultRole);
        _users.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User u, CancellationToken _) => u);
        _refreshTokens.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((RefreshToken t, CancellationToken _) => t);

        var result = await CreateHandler().Handle(
            new RegisterCommand("alice", "alice@example.com", "Password1!"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("alice", result.Username);
        Assert.Equal("alice@example.com", result.Email);

        _users.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(
                new RegisterCommand("alice", "alice@example.com", "Password1!"),
                CancellationToken.None));

        Assert.Equal("Email is already registered.", ex.Message);
        _users.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsInvalidOperationException()
    {
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(
                new RegisterCommand("alice", "alice@example.com", "Password1!"),
                CancellationToken.None));

        Assert.Equal("Username is already taken.", ex.Message);
        _users.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
