using Abstractions.Services;
using Contracts.DTOs.Authentication;
using Contracts.Dtos.RefreshTokenDtos;
using Contracts.Options;
using Microsoft.EntityFrameworkCore;
using Models.RefreshToken;
using Models.Users;
using Persistence.DbContext;

namespace Application.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenGenerationService _tokenGenerationService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthorizationService(
        ApplicationDbContext dbContext,
        ITokenGenerationService tokenGenerationService,
        IRefreshTokenService refreshTokenService,
        JwtOptions jwtOptions)
    {
        _dbContext = dbContext;
        _tokenGenerationService = tokenGenerationService;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Nickname == request.Nickname);

        if (user is null || !VerifyPassword(request.Password, user.Password))
            throw new UnauthorizedAccessException("Invalid login or password");

        var accessToken = _tokenGenerationService.GenerateAccessToken(user);
        var refreshToken = _tokenGenerationService.GenerateRefreshToken();

        var refreshDays = GetRefreshTokenLifetimeDays();

        await _refreshTokenService.CreateAsync(
            userId: user.Id,
            token: refreshToken,
            expiresAt: DateTime.UtcNow.AddDays(refreshDays));

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var exists = await _dbContext.Users
            .AnyAsync(x => x.Nickname == request.Nickname);

        if (exists)
            throw new InvalidOperationException("User with this nickname already exists");

        if (request.Nickname.Length < 3 || request.Nickname.Length > 16)
            throw new InvalidOperationException("Wrong nickname length");
        
        if (request.Password.Length < 10 || request.Password.Length > 16)
            throw new InvalidOperationException("Wrong password length");

        var user = new User
        {
            Nickname = request.Nickname,
            Password = HashPassword(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var accessToken = _tokenGenerationService.GenerateAccessToken(user);
        var refreshToken = _tokenGenerationService.GenerateRefreshToken();

        var refreshDays = GetRefreshTokenLifetimeDays();

        await _refreshTokenService.CreateAsync(
            userId: user.Id,
            token: refreshToken,
            expiresAt: DateTime.UtcNow.AddDays(refreshDays));

        return new RegisterResponseDto {  
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken
            
        };
    }

    public async Task<RefreshResponseDto> RefreshAsync(RefreshRequestDto request)
    {
        var existingRefreshToken = await _refreshTokenService.GetActiveAsync(request.RefreshToken);

        if (existingRefreshToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == existingRefreshToken.UserId);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        var newAccessToken = _tokenGenerationService.GenerateAccessToken(user);
        var newRefreshTokenValue = _tokenGenerationService.GenerateRefreshToken();

        var refreshDays = GetRefreshTokenLifetimeDays();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false,
            DeviceInfo = existingRefreshToken.DeviceInfo
        };

        await _refreshTokenService.ReplaceAsync(request.RefreshToken, newRefreshToken);

        return new RefreshResponseDto(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenValue);
    }

    public async Task LogoutAsync(LogoutRequestDto request)
    {
        await _refreshTokenService.RevokeAllByUserIdAsync(request.UserId);
    }

    private int GetRefreshTokenLifetimeDays()
    {
        int days = _jwtOptions.RefreshTokenExpirationDays;
        return days;
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string enteredPassword, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
    }
}