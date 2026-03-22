namespace BankSystem.Core.Entities;

public class Transaction
{
    public long TransactionId { get; set; }
    public TransactionType Type { get; set; }
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Account? FromAccount { get; set; }
    public virtual Account? ToAccount { get; set; }
    public virtual User? User { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Reversed
}
