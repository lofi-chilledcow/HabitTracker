using HabitService.DTOs;
using MediatR;

namespace HabitService.Commands;

public record UpsertHabitCompletionCommand(
    Guid HabitId,
    Guid UserId,
    DateOnly CompletedDate,
    string? Notes) : IRequest<HabitCompletionDto?>;
