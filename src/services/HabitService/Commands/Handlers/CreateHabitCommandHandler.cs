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
            Name = request.Name,
            Description = request.Description,
            Frequency = request.Frequency
        };

        db.Habits.Add(habit);
        await db.SaveChangesAsync(cancellationToken);

        return new HabitDto(habit.Id, habit.Name, habit.Description, habit.Frequency, habit.CreatedAt, habit.IsActive);
    }
}
