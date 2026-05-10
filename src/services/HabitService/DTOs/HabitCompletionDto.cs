namespace HabitService.DTOs;

public record HabitCompletionDto(
    Guid Id,
    Guid HabitId,
    DateOnly CompletedDate,
    string? Notes,
    DateTime CreatedAt
);

public record UpsertHabitCompletionDto(string? Notes = null);
