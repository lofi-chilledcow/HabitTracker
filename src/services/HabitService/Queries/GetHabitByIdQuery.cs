using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetHabitByIdQuery(Guid Id, Guid UserId) : IRequest<HabitDto?>;
