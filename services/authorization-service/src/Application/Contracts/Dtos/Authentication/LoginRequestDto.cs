namespace Contracts.DTOs.Authentication;

public class LoginRequestDto
{
    public required string Nickname { get; set; }
    
    public required string Password { get; set; }
}