namespace BankSystem.Core.Entities;

public class TwoFactorAuth
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public string? BackupCodes { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}

public class TwoFactorToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TwoFactorTokenType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}

public enum TwoFactorTokenType
{
    Setup,
    Login,
    PasswordReset,
    Transaction
}
