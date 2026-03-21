using AuthService;
using Gateway.Api.Dtos.Auth;
using Gateway.Api.GrpcClients.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthGrpcService _authGrpcService;
    
    public AuthController(AuthGrpcService authGrpcService)
    {
        _authGrpcService = authGrpcService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        LoginResponse response = await _authGrpcService.LoginAsync(loginDto);
        return Ok(response);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var response = await _authGrpcService.RegisterAsync(registerDto);
        return Ok(response);
    }
    
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] AccessTokenDto accessTokenDto)
    {
        var response = await _authGrpcService.ValidateAccessTokenAsync(accessTokenDto);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var response = await _authGrpcService.RefreshAccessTokenAsync(refreshTokenDto);
        return Ok(response);
    }
}