using AuthService.Commands;
using AuthService.Models;
using AuthService.Repositories;
using Moq;
using Xunit;

namespace AuthService.Tests.Commands;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();

    private GetCurrentUserQueryHandler CreateHandler() => new(_users.Object);

    [Fact]
    public async Task Handle_ActiveUser_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new User
              {
                  Id = userId,
                  Username = "alice",
                  Email = "alice@example.com",
                  PhoneNumber = "5551234567",
                  IsActive = true,
                  Role = new Role { Id = Guid.NewGuid(), Name = "User" }
              });

        var result = await CreateHandler().Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        Assert.Equal(userId, result.Id);
        Assert.Equal("alice", result.Username);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("5551234567", result.PhoneNumber);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new User
              {
                  Id = userId,
                  IsActive = false,
                  Role = new Role { Id = Guid.NewGuid(), Name = "User" }
              });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetCurrentUserQuery(userId), CancellationToken.None));
    }
}
