using MediatR;

namespace AuthService.Commands;

public record LogoutCommand(string RefreshToken) : IRequest;
