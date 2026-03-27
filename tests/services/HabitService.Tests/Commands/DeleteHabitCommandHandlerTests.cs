using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class DeleteHabitCommandHandlerTests
{
    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingHabit_RemovesFromDatabaseAndReturnsTrue()
    {
        using var db = CreateDb();
        var habit = new Habit { Name = "Exercise", Frequency = "Daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCommandHandler(db);
        var result = await handler.Handle(new DeleteHabitCommand(habit.Id), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(0, await db.Habits.CountAsync());
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsFalse()
    {
        using var db = CreateDb();
        var handler = new DeleteHabitCommandHandler(db);

        var result = await handler.Handle(new DeleteHabitCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result);
    }
}
