using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record SetUserStatusCommand(Guid UserId, bool IsActive) : IRequest<AdminUserDto?>;
