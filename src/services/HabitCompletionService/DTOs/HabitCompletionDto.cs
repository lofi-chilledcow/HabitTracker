namespace HabitCompletionService.DTOs;

public record HabitCompletionDto(
    Guid Id,
    Guid HabitId,
    Guid UserId,
    DateTime CompletedDate,
    string? Notes,
    DateTime CreatedAt
);

public record CreateHabitCompletionDto(
    Guid HabitId,
    DateTime? CompletedDate,
    string? Notes
);
