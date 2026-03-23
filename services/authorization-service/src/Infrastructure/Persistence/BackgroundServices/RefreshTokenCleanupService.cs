using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.DbContext;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expiredTokens = await dbContext.RefreshTokens
                    .Where(x => x.ExpiresAt <= DateTime.UtcNow || x.IsRevoked)
                    .ToListAsync(stoppingToken);

                if (expiredTokens.Any())
                {
                    dbContext.RefreshTokens.RemoveRange(expiredTokens);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "[RefreshTokenCleanup] Deleted {Count} expired/revoked refresh tokens.",
                        expiredTokens.Count);
                }
                else
                {
                    _logger.LogInformation(
                        "[RefreshTokenCleanup] No expired or revoked refresh tokens found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RefreshTokenCleanup] Error during cleanup.");
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}