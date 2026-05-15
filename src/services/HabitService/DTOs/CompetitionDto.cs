namespace HabitService.DTOs;

public record LeaderboardEntryDto(
    Guid HabitId,
    string Username,
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    int CompletionCount,
    DateTime CreatedAt
);
