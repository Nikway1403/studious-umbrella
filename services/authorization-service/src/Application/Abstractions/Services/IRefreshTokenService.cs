using Models.RefreshToken;

namespace Abstractions.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(long userId,
        string token,
        DateTime expiresAt,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default);
    
    Task<RefreshToken?> GetActiveAsync(string token,
        CancellationToken cancellationToken = default);
    
    Task RevokeAsync(string token,
        CancellationToken cancellationToken = default);
    
    Task RevokeAllByUserIdAsync(long userId,
        CancellationToken cancellationToken = default);
    
    Task ReplaceAsync(string oldToken, RefreshToken newRefreshToken,
        CancellationToken cancellationToken = default);
}