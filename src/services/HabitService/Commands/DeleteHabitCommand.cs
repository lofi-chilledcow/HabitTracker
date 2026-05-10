using MediatR;

namespace HabitService.Commands;

public record DeleteHabitCommand(Guid Id, Guid UserId) : IRequest<bool>;
