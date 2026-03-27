using HabitCompletionService.Data;
using HabitCompletionService.Models;
using HabitCompletionService.Queries;
using HabitCompletionService.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitCompletionService.Tests.Queries;

public class GetTodaysCompletionsQueryHandlerTests
{
    private static HabitCompletionDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitCompletionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithTodaysCompletions_ReturnsThem()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = userId,
            CompletedDate = DateTime.UtcNow.Date
        });
        await db.SaveChangesAsync();

        var handler = new GetTodaysCompletionsQueryHandler(db);
        var result = await handler.Handle(new GetTodaysCompletionsQuery(userId), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_WithYesterdaysCompletions_ReturnsEmpty()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = userId,
            CompletedDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var handler = new GetTodaysCompletionsQueryHandler(db);
        var result = await handler.Handle(new GetTodaysCompletionsQuery(userId), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ReturnsEmpty()
    {
        using var db = CreateDb();
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CompletedDate = DateTime.UtcNow.Date
        });
        await db.SaveChangesAsync();

        var handler = new GetTodaysCompletionsQueryHandler(db);
        var result = await handler.Handle(new GetTodaysCompletionsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }
}
