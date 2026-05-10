using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record ListUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>;
