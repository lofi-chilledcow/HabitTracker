using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetTodaysCompletionsQuery(Guid UserId, DateOnly Today) : IRequest<IReadOnlyList<HabitCompletionDto>>;
