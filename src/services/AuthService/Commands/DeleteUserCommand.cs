using MediatR;

namespace AuthService.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
