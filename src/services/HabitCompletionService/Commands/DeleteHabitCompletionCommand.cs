using MediatR;

namespace HabitCompletionService.Commands;

public record DeleteHabitCompletionCommand(Guid Id, Guid UserId) : IRequest<bool>;
