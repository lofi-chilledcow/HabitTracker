using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Commands.Handlers;

public class UpdateHabitCommandHandler(HabitDbContext db) : IRequestHandler<UpdateHabitCommand, HabitDto?>
{
    public async Task<HabitDto?> Handle(UpdateHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(
            h => h.Id == request.Id && h.UserId == request.UserId,
            cancellationToken);
        if (habit is null) return null;

        habit.Name = request.Name;
        habit.Description = request.Description;
        habit.Frequency = request.Frequency;
        habit.TargetDaysPerWeek = request.TargetDaysPerWeek;
        habit.IsPublic = request.IsPublic;
        habit.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new HabitDto(
            habit.Id,
            habit.Name,
            habit.Description,
            habit.Frequency,
            habit.TargetDaysPerWeek,
            habit.IsPublic,
            habit.CreatedAt,
            habit.UpdatedAt,
            habit.IsActive);
    }
}
