using HabitService.DTOs;
using MediatR;

namespace HabitService.Commands;

public record UpdateHabitCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic) : IRequest<HabitDto?>;
