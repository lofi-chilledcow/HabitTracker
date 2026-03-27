using HabitCompletionService.Data;
using HabitCompletionService.Models;
using HabitCompletionService.Queries;
using HabitCompletionService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitCompletionService.Tests.Queries;

public class GetCompletionsByHabitIdQueryHandlerTests
{
    private static HabitCompletionDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitCompletionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithCompletions_ReturnsForMatchingHabitId()
    {
        using var db = CreateDb();
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.HabitCompletions.AddRange(
            new HabitCompletion { HabitId = habitId, UserId = userId, CompletedDate = DateTime.UtcNow.Date },
            new HabitCompletion { HabitId = habitId, UserId = userId, CompletedDate = DateTime.UtcNow.Date.AddDays(-1) });
        await db.SaveChangesAsync();

        var handler = new GetCompletionsByHabitIdQueryHandler(db);
        var result = await handler.Handle(new GetCompletionsByHabitIdQuery(habitId), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal(habitId, r.HabitId));
    }

    [Fact]
    public async Task Handle_WithDifferentHabitId_ReturnsEmpty()
    {
        using var db = CreateDb();
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CompletedDate = DateTime.UtcNow.Date
        });
        await db.SaveChangesAsync();

        var handler = new GetCompletionsByHabitIdQueryHandler(db);
        var result = await handler.Handle(new GetCompletionsByHabitIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MultipleCompletions_OrderedByDateDescending()
    {
        using var db = CreateDb();
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;
        db.HabitCompletions.AddRange(
            new HabitCompletion { HabitId = habitId, UserId = userId, CompletedDate = today.AddDays(-2) },
            new HabitCompletion { HabitId = habitId, UserId = userId, CompletedDate = today },
            new HabitCompletion { HabitId = habitId, UserId = userId, CompletedDate = today.AddDays(-1) });
        await db.SaveChangesAsync();

        var handler = new GetCompletionsByHabitIdQueryHandler(db);
        var result = (await handler.Handle(new GetCompletionsByHabitIdQuery(habitId), CancellationToken.None)).ToList();

        Assert.Equal(today, result[0].CompletedDate);
        Assert.Equal(today.AddDays(-1), result[1].CompletedDate);
        Assert.Equal(today.AddDays(-2), result[2].CompletedDate);
    }
}
