using HabitCompletionService.Commands;
using HabitCompletionService.Commands.Handlers;
using HabitCompletionService.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitCompletionService.Tests.Commands;

public class CreateHabitCompletionCommandHandlerTests
{
    private static HabitCompletionDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitCompletionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        using var db = CreateDb();
        var handler = new CreateHabitCompletionCommandHandler(db);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        var result = await handler.Handle(
            new CreateHabitCompletionCommand(habitId, userId, date, "Felt great"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(habitId, result.HabitId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(date, result.CompletedDate);
        Assert.Equal("Felt great", result.Notes);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsToDatabase()
    {
        using var db = CreateDb();
        var handler = new CreateHabitCompletionCommandHandler(db);

        await handler.Handle(
            new CreateHabitCompletionCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date, null),
            CancellationToken.None);

        Assert.Equal(1, await db.HabitCompletions.CountAsync());
    }
}
