using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using MediatR;

namespace AuthService.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly JwtTokenService _jwt;

    public LoginCommandHandler(IUserRepository users, IRefreshTokenRepository refreshTokens, JwtTokenService jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByLoginIdentifierWithRoleAsync(request.Identifier, cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid login or password.");

        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            TokenHash = _jwt.HashRefreshToken(refreshTokenValue),
            UserId = user.Id,
            ExpiresAt = _jwt.GetRefreshTokenExpiry()
        };

        await _refreshTokens.CreateAsync(refreshToken, cancellationToken);

        return new AuthResponse(
            _jwt.Generate(user, user.Role.Name),
            refreshTokenValue,
            new UserProfileDto(user.Id, user.Username, user.Email, user.PhoneNumber, user.Role.Name));
    }
}
