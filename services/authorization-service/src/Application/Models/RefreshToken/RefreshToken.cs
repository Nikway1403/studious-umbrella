namespace Models.RefreshToken;

public class RefreshToken
{
    public long Id { get; init; }

    public long UserId { get; set; }

    public required string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? DeviceInfo { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}