using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetLeaderboardQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ReturnsOnlyPublicActiveHabits()
    {
        using var db = CreateDb();
        var publicHabit = new Habit { UserId = UserId, Name = "Public", Frequency = "daily", IsPublic = true };
        var privateHabit = new Habit { UserId = UserId, Name = "Private", Frequency = "daily", IsPublic = false };
        var archivedHabit = new Habit { UserId = UserId, Name = "Archived", Frequency = "daily", IsPublic = true, IsActive = false };
        db.Habits.AddRange(publicHabit, privateHabit, archivedHabit);
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardQueryHandler(db);

        var result = await handler.Handle(new GetLeaderboardQuery(), CancellationToken.None);

        var entry = Assert.Single(result);
        Assert.Equal(publicHabit.Id, entry.HabitId);
    }

    [Fact]
    public async Task Handle_OrdersByRecentCompletionCount()
    {
        using var db = CreateDb();
        var firstHabit = new Habit { UserId = UserId, Name = "First", Frequency = "daily", IsPublic = true, CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var secondHabit = new Habit { UserId = UserId, Name = "Second", Frequency = "daily", IsPublic = true, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Habits.AddRange(firstHabit, secondHabit);
        db.HabitCompletions.AddRange(
            new HabitCompletion { HabitId = firstHabit.Id, UserId = UserId, CompletedDate = today },
            new HabitCompletion { HabitId = secondHabit.Id, UserId = UserId, CompletedDate = today },
            new HabitCompletion { HabitId = secondHabit.Id, UserId = UserId, CompletedDate = today.AddDays(-1) });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardQueryHandler(db);

        var result = await handler.Handle(new GetLeaderboardQuery(), CancellationToken.None);

        Assert.Equal(secondHabit.Id, result[0].HabitId);
        Assert.Equal(2, result[0].CompletionCount);
    }
}
