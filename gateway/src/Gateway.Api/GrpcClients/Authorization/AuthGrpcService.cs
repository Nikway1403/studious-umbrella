using AuthService;
using Gateway.Api.Dtos.Auth;

namespace Gateway.Api.GrpcClients.Authorization;

public class AuthGrpcService
{
    private readonly AuthGrpc.AuthGrpcClient _client;
    
    public AuthGrpcService(AuthGrpc.AuthGrpcClient client)
    {
        _client = client;
    }
    
    public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
    {
        var request = new LoginRequest
        {
            Nickname = loginDto.Nickname,
            Password = loginDto.Password,
        };

        return await _client.LoginAsync(request);
    }
    
    public async Task<RegisterResponse> RegisterAsync(RegisterDto registerDto)
    {
        var request = new RegisterRequest
        {
            Nickname = registerDto.Nickname,
            Password = registerDto.Password,
        };

        return await _client.RegisterAsync(request);
    }
    
    public async Task<ValidateTokenResponse> ValidateAccessTokenAsync(AccessTokenDto accessTokenDto)
    {
        var request = new ValidateTokenRequest
        {
            AccessToken = accessTokenDto.AccessToken,
        };

        return await _client.ValidateTokenAsync(request);
    }

    public async Task<RefreshResponse> RefreshAccessTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var request = new RefreshRequest
        {
            RefreshToken = refreshTokenDto.RefreshToken,
        };
        
        return  await _client.RefreshAsync(request);
    }
}