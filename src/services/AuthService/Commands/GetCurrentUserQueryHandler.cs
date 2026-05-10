using AuthService.DTOs;
using AuthService.Repositories;
using MediatR;

namespace AuthService.Commands;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    private readonly IUserRepository _users;

    public GetCurrentUserQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdWithRoleAsync(request.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("User session is no longer valid.");

        return new UserProfileDto(user.Id, user.Username, user.Email, user.PhoneNumber, user.Role.Name);
    }
}
