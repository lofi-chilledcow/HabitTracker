using AuthService.DTOs;
using MediatR;

namespace AuthService.Commands;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserProfileDto>;
