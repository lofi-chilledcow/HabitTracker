using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class UpdateHabitCommandHandlerTests
{
    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingHabit_UpdatesAllFields()
    {
        using var db = CreateDb();
        var habit = new Habit { Name = "Old", Description = "Old desc", Frequency = "Daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpdateHabitCommandHandler(db);
        var result = await handler.Handle(
            new UpdateHabitCommand(habit.Id, "New", "New desc", "Weekly", false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal("New desc", result.Description);
        Assert.Equal("Weekly", result.Frequency);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        using var db = CreateDb();
        var handler = new UpdateHabitCommandHandler(db);

        var result = await handler.Handle(
            new UpdateHabitCommand(Guid.NewGuid(), "Name", null, "Daily", true),
            CancellationToken.None);

        Assert.Null(result);
    }
}
