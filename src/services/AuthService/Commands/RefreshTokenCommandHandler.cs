using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using MediatR;

namespace AuthService.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly JwtTokenService _jwt;

    public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokens, JwtTokenService jwt)
    {
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokens.GetByTokenHashAsync(
            _jwt.HashRefreshToken(request.RefreshToken),
            cancellationToken);

        if (existing is null || !existing.IsActive || !existing.User.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        await _refreshTokens.RevokeAsync(existing, cancellationToken);

        var newRefreshTokenValue = _jwt.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            TokenHash = _jwt.HashRefreshToken(newRefreshTokenValue),
            UserId = existing.UserId,
            ExpiresAt = _jwt.GetRefreshTokenExpiry()
        };

        await _refreshTokens.CreateAsync(newRefreshToken, cancellationToken);

        var accessToken = _jwt.Generate(existing.User, existing.User.Role.Name);

        return new AuthResponse(
            accessToken,
            newRefreshTokenValue,
            new UserProfileDto(existing.User.Id, existing.User.Username, existing.User.Email, existing.User.PhoneNumber, existing.User.Role.Name));
    }
}
