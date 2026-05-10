using MediatR;

namespace HabitService.Commands;

public enum DeleteHabitCompletionResult
{
    HabitNotFound,
    Deleted,
    AlreadyMissing
}

public record DeleteHabitCompletionCommand(
    Guid HabitId,
    Guid UserId,
    DateOnly CompletedDate) : IRequest<DeleteHabitCompletionResult>;
