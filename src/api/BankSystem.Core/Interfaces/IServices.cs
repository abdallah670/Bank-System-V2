using BankSystem.Core.Entities;

namespace BankSystem.Core.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Token, string? RefreshToken, User? User, string? Error)> LoginAsync(string username, string password, string ipAddress);
    Task<(bool Success, string? Token, string? Error)> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> LogoutAsync(int userId);
}

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(string username, string email, string password, string firstName, string lastName, UserRole role);
    Task<bool> UpdateRoleAsync(int id, UserRole role);
    Task<bool> DeleteAsync(int id);
    Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword);
}

public interface ICustomerService
{
    Task<Customer?> GetByIdAsync(int id);
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null);
    Task<Customer> CreateAsync(string firstName, string lastName, string email, string phone, string? address, string? city, string country, DateTime? dateOfBirth);
    Task<Customer?> UpdateAsync(int id, string firstName, string lastName, string email, string phone, string? address, string? city, string country);
    Task<bool> DeleteAsync(int id);
}

public interface IAccountService
{
    Task<Account?> GetByIdAsync(int id);
    Task<(IEnumerable<Account> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? customerId = null);
    Task<Account> CreateAsync(int customerId, string accountType, string currency, decimal initialBalance);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<decimal> GetTotalBalanceAsync();
}

public interface ITransactionService
{
    Task<Transaction> DepositAsync(int accountId, decimal amount, string? description, int userId);
    Task<Transaction> WithdrawAsync(int accountId, decimal amount, string? description, int userId);
    Task<(Transaction? FromTransaction, Transaction? ToTransaction)> TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? description, int userId);
    Task<Transaction?> GetByIdAsync(long id);
    Task<(IEnumerable<Transaction> Items, int TotalCount)> GetAllAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? type = null);
    Task<IEnumerable<Transaction>> GetRecentAsync(int count);
}

public interface IAuditService
{
    Task LogAsync(int userId, string action, string entityType, int? entityId, object? oldValues, object? newValues, string? ipAddress, string? userAgent);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? userId = null, string? action = null);
}

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
    Task<ChartData> GetChartDataAsync(int days = 30);
}

public class DashboardSummary
{
    public int TotalCustomers { get; set; }
    public int TotalAccounts { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal TodayDeposits { get; set; }
    public decimal TodayWithdrawals { get; set; }
    public int TodayTransactions { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

public class ChartData
{
    public List<TransactionTrend> TransactionTrends { get; set; } = new();
    public List<AccountDistribution> AccountTypeDistribution { get; set; } = new();
    public List<MonthlyBalance> BalanceTrends { get; set; } = new();
}

public class TransactionTrend
{
    public DateTime Date { get; set; }
    public decimal Deposits { get; set; }
    public decimal Withdrawals { get; set; }
    public decimal Transfers { get; set; }
}

public class AccountDistribution
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalBalance { get; set; }
}

public class MonthlyBalance
{
    public string Month { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
