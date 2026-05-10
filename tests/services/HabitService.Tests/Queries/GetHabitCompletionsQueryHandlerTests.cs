using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetHabitCompletionsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_OwnedHabit_ReturnsCompletionsNewestFirst()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        db.HabitCompletions.AddRange(
            new HabitCompletion { HabitId = habit.Id, UserId = UserId, CompletedDate = new DateOnly(2026, 5, 9) },
            new HabitCompletion { HabitId = habit.Id, UserId = UserId, CompletedDate = new DateOnly(2026, 5, 10) });
        await db.SaveChangesAsync();

        var handler = new GetHabitCompletionsQueryHandler(db);

        var result = await handler.Handle(new GetHabitCompletionsQuery(habit.Id, UserId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 5, 10), result[0].CompletedDate);
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsNull()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Read", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new GetHabitCompletionsQueryHandler(db);

        var result = await handler.Handle(new GetHabitCompletionsQuery(habit.Id, UserId), CancellationToken.None);

        Assert.Null(result);
    }
}
