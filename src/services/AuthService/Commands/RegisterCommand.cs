using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<AuthResponse>;
