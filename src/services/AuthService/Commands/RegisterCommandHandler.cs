using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using MediatR;

namespace AuthService.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly JwtTokenService _jwt;

    public RegisterCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        JwtTokenService jwt)
    {
        _users = users;
        _roles = roles;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email is already registered.");

        if (await _users.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new InvalidOperationException("Username is already taken.");

        var role = await _roles.GetByNameAsync("User", cancellationToken)
            ?? throw new InvalidOperationException("Default role not found.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id
        };

        await _users.CreateAsync(user, cancellationToken);

        var refreshToken = new RefreshToken
        {
            Token = _jwt.GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = _jwt.GetRefreshTokenExpiry()
        };

        await _refreshTokens.CreateAsync(refreshToken, cancellationToken);

        return new AuthResponse(_jwt.Generate(user, role.Name), refreshToken.Token, user.Username, user.Email);
    }
}
