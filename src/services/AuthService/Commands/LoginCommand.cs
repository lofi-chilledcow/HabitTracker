using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record LoginCommand(string Identifier, string Password) : IRequest<AuthResponse>;
