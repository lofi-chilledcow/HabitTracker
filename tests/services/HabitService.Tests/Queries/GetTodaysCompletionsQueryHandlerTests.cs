using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetTodaysCompletionsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersCompletionsForToday()
    {
        using var db = CreateDb();
        var today = new DateOnly(2026, 5, 10);
        var yesterday = today.AddDays(-1);
        var habit = new Habit { UserId = UserId, Name = "Read", Frequency = "daily" };
        var otherHabit = new Habit { UserId = OtherUserId, Name = "Run", Frequency = "daily" };
        db.Habits.AddRange(habit, otherHabit);
        db.HabitCompletions.AddRange(
            new HabitCompletion { HabitId = habit.Id, UserId = UserId, CompletedDate = today },
            new HabitCompletion { HabitId = habit.Id, UserId = UserId, CompletedDate = yesterday },
            new HabitCompletion { HabitId = otherHabit.Id, UserId = OtherUserId, CompletedDate = today });
        await db.SaveChangesAsync();

        var handler = new GetTodaysCompletionsQueryHandler(db);

        var result = await handler.Handle(new GetTodaysCompletionsQuery(UserId, today), CancellationToken.None);

        var completion = Assert.Single(result);
        Assert.Equal(habit.Id, completion.HabitId);
        Assert.Equal(today, completion.CompletedDate);
    }
}
