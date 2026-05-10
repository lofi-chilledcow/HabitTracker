using AuthService.Models;

namespace AuthService.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<bool> RevokeByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
