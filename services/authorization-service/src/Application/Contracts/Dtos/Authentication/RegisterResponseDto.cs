namespace Contracts.DTOs.Authentication;

public class RegisterResponseDto
{
    public required long UserId { get; set; }

    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}