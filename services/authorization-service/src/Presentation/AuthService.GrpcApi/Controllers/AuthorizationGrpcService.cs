using Abstractions.Services;
using Contracts.DTOs.Authentication;
using Contracts.Dtos.RefreshTokenDtos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AuthService.GrpcApi.Controllers;

public class AuthorizationGrpcService : AuthGrpc.AuthGrpcBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidationService _validationService;

    public AuthorizationGrpcService(
        IAuthorizationService authorizationService,
        IValidationService validationService)
    {
        _authorizationService = authorizationService;
        _validationService = validationService;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var dto = new LoginRequestDto
        {
            Nickname = request.Nickname,
            Password = request.Password
        };

        var result = await _authorizationService.LoginAsync(dto);

        return new LoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var dto = new RegisterRequestDto
        {
            Nickname = request.Nickname,
            Password = request.Password
        };

        var result = await _authorizationService.RegisterAsync(dto);

        return new RegisterResponse
        {
            UserId = result.UserId,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };
    }

    public override Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request,
        ServerCallContext context)
    {
        var result = _validationService.ValidateAccessToken(request.AccessToken);

        return Task.FromResult(new ValidateTokenResponse
        {
            IsValid = result.IsValid
        });
    }

    public override async Task<RefreshResponse> Refresh(RefreshRequest request, ServerCallContext context)
    {
        var dto = new RefreshRequestDto(request.RefreshToken);
        var result = await _authorizationService.RefreshAsync(dto);

        return new RefreshResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };
    }

    public override async Task<Empty> Logout(LogoutRequest request, ServerCallContext context)
    {
        var dto = new LogoutRequestDto(request.UserId);
        await _authorizationService.LogoutAsync(dto);

        return new Empty();
    }
}