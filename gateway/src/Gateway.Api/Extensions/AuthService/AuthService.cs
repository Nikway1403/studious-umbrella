using AuthService;
using Gateway.Api.GrpcClients.Authorization;
using Gateway.Api.Options;

namespace Gateway.Api.Extensions.AuthService;

public static class AuthService
{
    public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("GrpcOptions"));
        
        GrpcOptions? grpcOptions = configuration
            .GetSection("GrpcOptions")
            .Get<GrpcOptions>();
        
        if (string.IsNullOrEmpty(grpcOptions.AuthServiceUrl))
        {
            throw new ArgumentNullException(nameof(grpcOptions.AuthServiceUrl), "AuthServiceUrl not found in configuration.");
        }

        services.AddGrpcClient<AuthGrpc.AuthGrpcClient>(o => 
        { 
            o.Address = new Uri(grpcOptions.AuthServiceUrl); 
        });
        
        services.AddScoped<AuthGrpcService>();
        
        return services;
    }
}