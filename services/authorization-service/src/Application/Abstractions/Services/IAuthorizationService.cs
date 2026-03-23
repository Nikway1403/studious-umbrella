using Contracts.DTOs.Authentication;
using Contracts.Dtos.RefreshTokenDtos;

namespace Abstractions.Services;

public interface IAuthorizationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    
    Task<RefreshResponseDto> RefreshAsync(RefreshRequestDto request);
    
    Task LogoutAsync(LogoutRequestDto request);
}