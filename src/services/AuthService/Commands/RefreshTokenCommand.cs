using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
