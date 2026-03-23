using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
