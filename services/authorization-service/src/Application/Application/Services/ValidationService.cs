using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Abstractions.Services;
using Contracts.DTOs.ValidationDtos;
using Contracts.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class ValidationService : IValidationService
{
    private readonly JwtOptions _jwtOptions;

    public ValidationService(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }
    
    public ValidationDto ValidateAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var entity = new ValidationDto
            {
                IsValid =  true,
                UserId = Convert.ToInt64(userIdClaim),
            };
            return entity;
        }
        catch
        {
            return new ValidationDto
            {
                IsValid = false,
                UserId = 0
            };
        }
    }
}