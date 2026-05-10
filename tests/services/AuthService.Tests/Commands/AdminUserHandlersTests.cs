using AuthService.Commands;
using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Commands;

public class AdminUserHandlersTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task ListUsers_ReturnsUsersWithRoles()
    {
        using var db = CreateDb();
        var userRole = new Role { Name = "User" };
        db.Roles.Add(userRole);
        db.Users.Add(new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = userRole
        });
        await db.SaveChangesAsync();

        var result = await new ListUsersQueryHandler(db).Handle(new ListUsersQuery(), CancellationToken.None);

        var user = Assert.Single(result);
        Assert.Equal("alice", user.Username);
        Assert.Equal("User", user.Role);
    }

    [Fact]
    public async Task SetUserStatus_ExistingUser_UpdatesIsActive()
    {
        using var db = CreateDb();
        var userRole = new Role { Name = "User" };
        var user = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hash",
            IsActive = true,
            Role = userRole
        };
        db.Roles.Add(userRole);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await new SetUserStatusCommandHandler(db).Handle(
            new SetUserStatusCommand(user.Id, false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsActive);
        Assert.False((await db.Users.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task SetUserRole_ExistingUser_UpdatesRole()
    {
        using var db = CreateDb();
        var userRole = new Role { Name = "User" };
        var adminRole = new Role { Name = "Admin" };
        var user = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = userRole
        };
        db.Roles.AddRange(userRole, adminRole);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await new SetUserRoleCommandHandler(db).Handle(
            new SetUserRoleCommand(user.Id, "Admin"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task SetUserRole_InvalidRole_ThrowsArgumentException()
    {
        using var db = CreateDb();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            new SetUserRoleCommandHandler(db).Handle(
                new SetUserRoleCommand(Guid.NewGuid(), "Owner"),
                CancellationToken.None));

        Assert.Equal("Role", ex.ParamName);
    }
}
