namespace HabitService.DTOs;

public record HabitDto(
    Guid Id,
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive
);

public record CreateHabitDto(
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic
);

public record UpdateHabitDto(
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic
);
