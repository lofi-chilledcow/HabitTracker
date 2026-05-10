namespace AuthService.DTOs;

public record AdminUserDto(
    Guid Id,
    string Username,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateUserStatusRequest(bool IsActive);

public record UpdateUserRoleRequest(string Role);
