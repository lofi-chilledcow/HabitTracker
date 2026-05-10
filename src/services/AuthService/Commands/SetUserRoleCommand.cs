using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record SetUserRoleCommand(Guid UserId, string Role) : IRequest<AdminUserDto?>;
