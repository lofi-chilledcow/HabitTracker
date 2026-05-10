using HabitService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class DeleteHabitCompletionCommandHandler(HabitDbContext db)
    : IRequestHandler<DeleteHabitCompletionCommand, DeleteHabitCompletionResult>
{
    public async Task<DeleteHabitCompletionResult> Handle(DeleteHabitCompletionCommand request, CancellationToken cancellationToken)
    {
        var habitExists = await db.Habits.AnyAsync(
            h => h.Id == request.HabitId && h.UserId == request.UserId && h.IsActive,
            cancellationToken);

        if (!habitExists)
            return DeleteHabitCompletionResult.HabitNotFound;

        var completion = await db.HabitCompletions.FirstOrDefaultAsync(
            c => c.HabitId == request.HabitId
                 && c.UserId == request.UserId
                 && c.CompletedDate == request.CompletedDate,
            cancellationToken);

        if (completion is null)
            return DeleteHabitCompletionResult.AlreadyMissing;

        db.HabitCompletions.Remove(completion);
        await db.SaveChangesAsync(cancellationToken);

        return DeleteHabitCompletionResult.Deleted;
    }
}
