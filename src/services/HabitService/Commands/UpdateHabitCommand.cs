using HabitService.DTOs;
using MediatR;

namespace HabitService.Commands;

public record UpdateHabitCommand(Guid Id, string Name, string? Description, string Frequency, bool IsActive) : IRequest<HabitDto?>;
