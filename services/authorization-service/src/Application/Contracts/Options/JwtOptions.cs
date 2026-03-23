namespace Contracts.Options;

public class JwtOptions
{
    public required string SecretKey { get; init; }
    
    public int AccessTokenExpirationMinutes { get; init; }
    
    public string? Issuer { get; init; }
    
    public string? Audience { get; init; }
}