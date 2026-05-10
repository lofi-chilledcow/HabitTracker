using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetAllHabitsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithMultipleUsers_ReturnsCurrentUsersActiveHabits()
    {
        using var db = CreateDb();
        db.Habits.AddRange(
            new Habit { UserId = UserId, Name = "Exercise", Frequency = "daily" },
            new Habit { UserId = UserId, Name = "Archived", Frequency = "daily", IsActive = false },
            new Habit { UserId = OtherUserId, Name = "Read", Frequency = "weekly" });
        await db.SaveChangesAsync();

        var handler = new GetAllHabitsQueryHandler(db);
        var result = await handler.Handle(new GetAllHabitsQuery(UserId), CancellationToken.None);

        var habit = Assert.Single(result);
        Assert.Equal("Exercise", habit.Name);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmpty()
    {
        using var db = CreateDb();
        var handler = new GetAllHabitsQueryHandler(db);

        var result = await handler.Handle(new GetAllHabitsQuery(UserId), CancellationToken.None);

        Assert.Empty(result);
    }
}
