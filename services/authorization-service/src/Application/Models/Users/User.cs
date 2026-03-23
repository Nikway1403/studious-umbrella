namespace Models.Users;

public class User
{
    public long Id { get; init; }

    public required string Nickname { get; set; }
    
    public required string Password { get; set; }

    public UsersRole Role { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}