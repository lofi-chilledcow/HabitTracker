using HabitService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class DeleteHabitCommandHandler(HabitDbContext db) : IRequestHandler<DeleteHabitCommand, bool>
{
    public async Task<bool> Handle(DeleteHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);
        if (habit is null) return false;

        db.Habits.Remove(habit);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
