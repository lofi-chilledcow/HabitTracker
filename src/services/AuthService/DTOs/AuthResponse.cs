namespace AuthService.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, string Username, string Email);
