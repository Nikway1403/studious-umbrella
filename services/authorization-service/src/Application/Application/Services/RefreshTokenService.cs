using Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using Models.RefreshToken;
using Persistence.DbContext;

namespace Application.Services;

public class RefreshTokenService: IRefreshTokenService
{
    private readonly ApplicationDbContext _dbContext;
    
    public RefreshTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<RefreshToken> CreateAsync(
        long userId,
        string token,
        DateTime expiresAt,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            DeviceInfo = deviceInfo
        };

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task RevokeAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (refreshToken is null)
            return;

        refreshToken.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceAsync(
        string oldToken,
        RefreshToken newRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var oldRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == oldToken, cancellationToken);

        if (oldRefreshToken is null)
            throw new InvalidOperationException("Old refresh token not found");

        oldRefreshToken.IsRevoked = true;
        _dbContext.RefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}