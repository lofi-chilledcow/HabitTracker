using HabitCompletionService.Commands;
using HabitCompletionService.Commands.Handlers;
using HabitCompletionService.Data;
using HabitCompletionService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HabitCompletionService.Tests.Commands;

public class DeleteHabitCompletionCommandHandlerTests
{
    private static HabitCompletionDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<HabitCompletionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ExistingCompletion_RemovesAndReturnsTrue()
    {
        using var db = CreateDb();
        var userId = Guid.NewGuid();
        var completion = new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = userId,
            CompletedDate = DateTime.UtcNow.Date
        };
        db.HabitCompletions.Add(completion);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCompletionCommandHandler(db);
        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(completion.Id, userId),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(0, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_WrongUserId_ReturnsFalse()
    {
        using var db = CreateDb();
        var completion = new HabitCompletion
        {
            HabitId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CompletedDate = DateTime.UtcNow.Date
        };
        db.HabitCompletions.Add(completion);
        await db.SaveChangesAsync();

        var handler = new DeleteHabitCompletionCommandHandler(db);
        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(completion.Id, Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
        Assert.Equal(1, await db.HabitCompletions.CountAsync());
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsFalse()
    {
        using var db = CreateDb();
        var handler = new DeleteHabitCompletionCommandHandler(db);

        var result = await handler.Handle(
            new DeleteHabitCompletionCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
