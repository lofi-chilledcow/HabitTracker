using HabitCompletionService.DTOs;
using MediatR;

namespace HabitCompletionService.Queries;

public record GetTodaysCompletionsQuery(Guid UserId) : IRequest<IEnumerable<HabitCompletionDto>>;
