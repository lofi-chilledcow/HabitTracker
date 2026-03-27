using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetAllHabitsQueryHandlerTests
{
    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithTwoHabits_ReturnsBoth()
    {
        using var db = CreateDb();
        db.Habits.AddRange(
            new Habit { Name = "Exercise", Frequency = "Daily" },
            new Habit { Name = "Read", Frequency = "Weekly" });
        await db.SaveChangesAsync();

        var handler = new GetAllHabitsQueryHandler(db);
        var result = await handler.Handle(new GetAllHabitsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmpty()
    {
        using var db = CreateDb();
        var handler = new GetAllHabitsQueryHandler(db);

        var result = await handler.Handle(new GetAllHabitsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
