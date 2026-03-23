using MediatR;

namespace HabitService.Commands;

public record DeleteHabitCommand(Guid Id) : IRequest<bool>;
