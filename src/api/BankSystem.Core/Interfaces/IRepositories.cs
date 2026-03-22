using BankSystem.Core.Entities;

namespace BankSystem.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<(IEnumerable<Account> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? customerId = null);
    Task<Account?> GetByIdWithLockAsync(int id);
    Task<Account> CreateAsync(Account account);
    Task<Account> UpdateAsync(Account account);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<decimal> GetTotalBalanceAsync();
}

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(long id);
    Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber);
    Task<(IEnumerable<Transaction> Items, int TotalCount)> GetAllAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? type = null);
    Task<(IEnumerable<Transaction> Items, int TotalCount)> GetByAccountIdAsync(int accountId, int page, int pageSize);
    Task<IEnumerable<Transaction>> GetRecentAsync(int count);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<decimal> GetTodayTotalByTypeAsync(string type);
    Task<decimal> GetTotalByTypeInRangeAsync(string type, DateTime start, DateTime end);
}

public interface IAuditLogRepository
{
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? userId = null, string? action = null);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int count);
}
