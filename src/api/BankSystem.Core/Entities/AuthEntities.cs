namespace BankSystem.Core.Entities;

public class RefreshToken
{
    public int TokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt != null;

    public virtual User? User { get; set; }
}

public class LoginAttempt
{
    public int AttemptId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? FailureReason { get; set; }
}
