using HabitService.Data;
using HabitService.DTOs;
using HabitService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class UpsertHabitCompletionCommandHandler(HabitDbContext db)
    : IRequestHandler<UpsertHabitCompletionCommand, HabitCompletionDto?>
{
    public async Task<HabitCompletionDto?> Handle(UpsertHabitCompletionCommand request, CancellationToken cancellationToken)
    {
        var habitExists = await db.Habits.AnyAsync(
            h => h.Id == request.HabitId && h.UserId == request.UserId && h.IsActive,
            cancellationToken);

        if (!habitExists)
            return null;

        var completion = await db.HabitCompletions.FirstOrDefaultAsync(
            c => c.HabitId == request.HabitId && c.CompletedDate == request.CompletedDate,
            cancellationToken);

        if (completion is null)
        {
            completion = new HabitCompletion
            {
                HabitId = request.HabitId,
                UserId = request.UserId,
                CompletedDate = request.CompletedDate,
                Notes = request.Notes
            };

            db.HabitCompletions.Add(completion);
        }
        else
        {
            completion.Notes = request.Notes;
        }

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(completion);
    }

    private static HabitCompletionDto ToDto(HabitCompletion completion) =>
        new(
            completion.Id,
            completion.HabitId,
            completion.CompletedDate,
            completion.Notes,
            completion.CreatedAt);
}
