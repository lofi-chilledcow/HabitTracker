using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetAllHabitsQuery(Guid UserId) : IRequest<IEnumerable<HabitDto>>;
