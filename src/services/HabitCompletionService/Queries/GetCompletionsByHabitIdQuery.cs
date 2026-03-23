using HabitCompletionService.DTOs;
using MediatR;

namespace HabitCompletionService.Queries;

public record GetCompletionsByHabitIdQuery(Guid HabitId) : IRequest<IEnumerable<HabitCompletionDto>>;
