using HabitService.Data;
using HabitService.Models;
using HabitService.Queries;
using HabitService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitService.Tests.Queries;

public class GetHabitByIdQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static HabitDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = UserId, Name = "Meditate", Description = "10 minutes", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new GetHabitByIdQueryHandler(db);
        var result = await handler.Handle(new GetHabitByIdQuery(habit.Id, UserId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(habit.Id, result.Id);
        Assert.Equal("Meditate", result.Name);
        Assert.Equal("daily", result.Frequency);
    }

    [Fact]
    public async Task Handle_OtherUsersHabit_ReturnsNull()
    {
        using var db = CreateDb();
        var habit = new Habit { UserId = OtherUserId, Name = "Meditate", Frequency = "daily" };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        var handler = new GetHabitByIdQueryHandler(db);
        var result = await handler.Handle(new GetHabitByIdQuery(habit.Id, UserId), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        using var db = CreateDb();
        var handler = new GetHabitByIdQueryHandler(db);

        var result = await handler.Handle(new GetHabitByIdQuery(Guid.NewGuid(), UserId), CancellationToken.None);

        Assert.Null(result);
    }
}
