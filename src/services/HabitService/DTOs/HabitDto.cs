namespace HabitService.DTOs;

public record HabitDto(
    Guid Id,
    string Name,
    string? Description,
    string Frequency,
    DateTime CreatedAt,
    bool IsActive
);

public record CreateHabitDto(
    string Name,
    string? Description,
    string Frequency
);

public record UpdateHabitDto(
    string Name,
    string? Description,
    string Frequency,
    bool IsActive
);
