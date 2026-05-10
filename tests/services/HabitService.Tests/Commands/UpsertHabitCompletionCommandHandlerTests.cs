using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class UpsertHabitCompletionCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_OwnedActiveHabit_CreatesCompletion()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpsertHabitCompletionCommandHandler(db);
        var date = new DateOnly(2026, 5, 10);

        var result = await handler.Handle(
            new UpsertHabitCompletionCommand(habit.Id, UserId, date, "Done"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(habit.Id, result.HabitId);
        Assert.Equal(date, result.CompletedDate);
        Assert.Equal("Done", result.Notes);
        Assert.Equal(1, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_ExistingCompletion_UpdatesNotesWithoutDuplicate()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        var date = new DateOnly(2026, 5, 10);
        db.Habits.Add(habit);
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = habit.Id,
            UserId = UserId,
            CompletedDate = date,
            Notes = "Old"
        });
        await db.SaveChangesAsync();

        var handler = new UpsertHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new UpsertHabitCompletionCommand(habit.Id, UserId, date, "New"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New", result.Notes);
        Assert.Equal(1, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsNull()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpsertHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new UpsertHabitCompletionCommand(habit.Id, UserId, new DateOnly(2026, 5, 10), null),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_InactiveHabit_ReturnsNull()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily", IsActive = false };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpsertHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new UpsertHabitCompletionCommand(habit.Id, UserId, new DateOnly(2026, 5, 10), null),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, await db.HabitCompletions.CountAsync());
    }
}
