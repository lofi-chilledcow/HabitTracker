using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class UpdateHabitCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingHabit_UpdatesAllFields()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Old", Description = "Old desc", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpdateHabitCommandHandler(db);
        var result = await handler.Handle(
            new UpdateHabitCommand(habit.Id, UserId, "New", "New desc", "weekly", 3, true),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal("New desc", result.Description);
        Assert.Equal("weekly", result.Frequency);
        Assert.Equal((byte?)3, result.TargetDaysPerWeek);
        Assert.True(result.IsPublic);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsNull()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Old", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpdateHabitCommandHandler(db);
        var result = await handler.Handle(
            new UpdateHabitCommand(habit.Id, UserId, "New", null, "daily", null, false),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        using var db = CreateDb();
        var handler = new UpdateHabitCommandHandler(db);

        var result = await handler.Handle(
            new UpdateHabitCommand(Guid.NewGuid(), UserId, "Name", null, "daily", null, false),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_InvalidFrequency_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Exercise", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new UpdateHabitCommandHandler(db);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(
                new UpdateHabitCommand(habit.Id, UserId, "Exercise", null, "monthly", null, false),
                CancellationToken.None));

        Assert.Equal("frequency", ex.ParamName);
    }
}
