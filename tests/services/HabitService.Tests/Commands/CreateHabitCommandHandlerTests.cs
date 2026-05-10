using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class CreateHabitCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsHabitDto()
    {
        using var db = CreateDb();
        var handler = new CreateHabitCommandHandler(db);

        var result = await handler.Handle(
            new CreateHabitCommand(UserId, "Exercise", "Morning run", "daily", null, true),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Exercise", result.Name);
        Assert.Equal("Morning run", result.Description);
        Assert.Equal("daily", result.Frequency);
        Assert.True(result.IsPublic);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsToDatabase()
    {
        using var db = CreateDb();
        var handler = new CreateHabitCommandHandler(db);

        await handler.Handle(
            new CreateHabitCommand(UserId, "Read", null, "daily", null, false),
            CancellationToken.None);

        Assert.Equal(1, await db.Habits.CountAsync());
        Assert.Equal(UserId, (await db.Habits.SingleAsync()).UserId);
    }
}
