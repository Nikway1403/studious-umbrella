using Abstractions.Services;
using Application.Services;
using Contracts.Options;
using Microsoft.EntityFrameworkCore;
using Persistence.BackgroundServices;
using Persistence.DbContext;

namespace AuthService.GrpcApi.Extensions;

public static class AuthApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
        
        var dbOptions = new DatabaseOptions();
        configuration.GetSection("DatabaseOptions").Bind(dbOptions);
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dbOptions.ConnectionString));

        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        services.AddScoped<IValidationService, ValidationService>();
        
        services.AddHostedService<MigrationBackgroundService>();
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}