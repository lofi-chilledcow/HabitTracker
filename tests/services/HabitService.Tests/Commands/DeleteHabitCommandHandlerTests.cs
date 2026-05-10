using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class DeleteHabitCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingHabit_ArchivesAndReturnsTrue()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Exercise", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCommandHandler(db);
        var result = await handler.Handle(new DeleteHabitCommand(habit.Id, UserId), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(1, await db.Habits.CountAsync());
        Assert.False((await db.Habits.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsFalse()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Exercise", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCommandHandler(db);
        var result = await handler.Handle(new DeleteHabitCommand(habit.Id, UserId), CancellationToken.None);

        Assert.False(result);
        Assert.True((await db.Habits.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsFalse()
    {
        using var db = CreateDb();
        var handler = new DeleteHabitCommandHandler(db);

        var result = await handler.Handle(new DeleteHabitCommand(Guid.NewGuid(), UserId), CancellationToken.None);

        Assert.False(result);
    }
}
