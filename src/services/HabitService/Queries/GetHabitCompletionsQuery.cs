using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetHabitCompletionsQuery(Guid HabitId, Guid UserId) : IRequest<IReadOnlyList<HabitCompletionDto>?>;
