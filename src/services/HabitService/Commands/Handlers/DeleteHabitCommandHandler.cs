using HabitService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class DeleteHabitCommandHandler(HabitDbContext db) : IRequestHandler<DeleteHabitCommand, bool>
{
    public async Task<bool> Handle(DeleteHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(
            h => h.Id == request.Id && h.UserId == request.UserId,
            cancellationToken);
        if (habit is null) return false;

        habit.IsActive = false;
        habit.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
