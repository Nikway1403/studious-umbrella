using Models.Users;

namespace Abstractions.Services;

public interface ITokenGenerationService
{
    string GenerateAccessToken(User user);
    
    string GenerateRefreshToken();
}