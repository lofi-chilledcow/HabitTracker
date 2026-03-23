using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class UpdateHabitCommandHandler(HabitDbContext db) : IRequestHandler<UpdateHabitCommand, HabitDto?>
{
    public async Task<HabitDto?> Handle(UpdateHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);
        if (habit is null) return null;

        habit.Name = request.Name;
        habit.Description = request.Description;
        habit.Frequency = request.Frequency;
        habit.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return new HabitDto(habit.Id, habit.Name, habit.Description, habit.Frequency, habit.CreatedAt, habit.IsActive);
    }
}
