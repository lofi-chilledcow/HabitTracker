using HabitCompletionService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitCompletionService.Commands.Handlers;

public class DeleteHabitCompletionCommandHandler(HabitCompletionDbContext db)
    : IRequestHandler<DeleteHabitCompletionCommand, bool>
{
    public async Task<bool> Handle(DeleteHabitCompletionCommand request, CancellationToken cancellationToken)
    {
        var completion = await db.HabitCompletions
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken);

        if (completion is null) return false;

        db.HabitCompletions.Remove(completion);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
