namespace BankSystem.Core.Entities;

public class Account
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public AccountType AccountType { get; set; } = AccountType.Savings;
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public virtual Customer? Customer { get; set; }
    public virtual ICollection<Transaction> OutgoingTransactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> IncomingTransactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    Savings,
    Checking,
    Business
}
