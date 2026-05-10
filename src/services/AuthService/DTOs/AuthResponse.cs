namespace AuthService.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, UserProfileDto User);

public record UserProfileDto(Guid Id, string Username, string Email, string? PhoneNumber, string Role);
