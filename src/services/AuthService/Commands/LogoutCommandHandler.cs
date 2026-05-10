using AuthService.Repositories;
using AuthService.Services;
using MediatR;

namespace AuthService.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly JwtTokenService _jwt;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, JwtTokenService jwt)
    {
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwt.HashRefreshToken(request.RefreshToken);
        await _refreshTokens.RevokeByTokenHashAsync(tokenHash, cancellationToken);
    }
}
