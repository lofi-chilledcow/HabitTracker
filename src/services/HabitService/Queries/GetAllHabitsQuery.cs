using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetAllHabitsQuery : IRequest<IEnumerable<HabitDto>>;
