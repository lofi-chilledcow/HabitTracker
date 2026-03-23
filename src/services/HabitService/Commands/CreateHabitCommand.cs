using HabitService.DTOs;
using MediatR;

namespace HabitService.Commands;

public record CreateHabitCommand(string Name, string? Description, string Frequency) : IRequest<HabitDto>;
