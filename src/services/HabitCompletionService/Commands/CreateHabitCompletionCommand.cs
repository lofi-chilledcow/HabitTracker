using HabitCompletionService.DTOs;
using MediatR;

namespace HabitCompletionService.Commands;

public record CreateHabitCompletionCommand(
    Guid HabitId,
    Guid UserId,
    DateTime CompletedDate,
    string? Notes
) : IRequest<HabitCompletionDto>;
