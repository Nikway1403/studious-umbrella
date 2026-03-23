using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.DbContext;

namespace Persistence.BackgroundServices;

public class MigrationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MigrationBackgroundService> _logger;

    public MigrationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MigrationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[MigrationService] Checking and applying pending migrations...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await dbContext.Database.GetPendingMigrationsAsync(stoppingToken)).Any())
            {
                _logger.LogInformation("[MigrationService] Applying migrations...");
                await dbContext.Database.MigrateAsync(stoppingToken);
                _logger.LogInformation("[MigrationService] Migrations applied successfully.");
            }
            else
            {
                _logger.LogInformation("[MigrationService] No pending migrations found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MigrationService] Error applying migrations.");
        }
    }
}