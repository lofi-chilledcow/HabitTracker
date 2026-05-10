using HabitService.Data;
using HabitService.DTOs;
using HabitService.Models;
using MediatR;

namespace HabitService.Commands.Handlers;

public class CreateHabitCommandHandler(HabitDbContext db) : IRequestHandler<CreateHabitCommand, HabitDto>
{
    public async Task<HabitDto> Handle(CreateHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = new Habit
        {
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            Frequency = request.Frequency,
            TargetDaysPerWeek = request.TargetDaysPerWeek,
            IsPublic = request.IsPublic
        };

        db.Habits.Add(habit);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(habit);
    }

    private static HabitDto ToDto(Habit habit) =>
        new(
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
