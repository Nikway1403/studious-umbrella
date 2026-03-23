namespace Contracts.DTOs.Authentication;

public class RegisterRequestDto
{
    public required string Nickname { get; set; }
    
    public required string Password { get; set; }
}