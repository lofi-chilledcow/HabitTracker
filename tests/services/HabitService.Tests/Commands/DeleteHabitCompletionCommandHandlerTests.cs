using HabitService.Commands;
using HabitService.Commands.Handlers;
using HabitService.Data;
using HabitService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Commands;

public class DeleteHabitCompletionCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingCompletion_DeletesCompletion()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        var date = new DateOnly(2026, 5, 10);
        db.Habits.Add(habit);
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = habit.Id,
            UserId = UserId,
            CompletedDate = date
        });
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(habit.Id, UserId, date),
            CancellationToken.None);

        Assert.Equal(DeleteHabitCompletionResult.Deleted, result);
        Assert.Equal(0, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_MissingCompletion_ReturnsAlreadyMissing()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(habit.Id, UserId, new DateOnly(2026, 5, 10)),
            CancellationToken.None);

        Assert.Equal(DeleteHabitCompletionResult.AlreadyMissing, result);
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsHabitNotFound()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(habit.Id, UserId, new DateOnly(2026, 5, 10)),
            CancellationToken.None);

        Assert.Equal(DeleteHabitCompletionResult.HabitNotFound, result);
    }
}
