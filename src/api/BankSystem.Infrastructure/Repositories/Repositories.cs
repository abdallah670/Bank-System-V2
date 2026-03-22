using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BankDbContext _context;

    public UserRepository(BankDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.LastModified = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        user.IsDeleted = true;
        user.IsActive = false;
        user.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Users.AnyAsync(u => u.UserId == id);

    public async Task<bool> UsernameExistsAsync(string username) =>
        await _context.Users.AnyAsync(u => u.Username == username);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);
}

public class CustomerRepository : ICustomerRepository
{
    private readonly BankDbContext _context;

    public CustomerRepository(BankDbContext context) => _context = context;

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _context.Customers.Include(c => c.Accounts).FirstOrDefaultAsync(c => c.CustomerId == id);

    public async Task<Customer?> GetByEmailAsync(string email) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Phone.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        customer.LastModified = DateTime.UtcNow;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;
        customer.IsDeleted = true;
        customer.IsActive = false;
        customer.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Customers.AnyAsync(c => c.CustomerId == id);
}

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext _context;

    public AccountRepository(BankDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(int id) =>
        await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.AccountId == id);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber) =>
        await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<(IEnumerable<Account> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? customerId = null)
    {
        var query = _context.Accounts.Include(a => a.Customer).AsQueryable();

        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Account?> GetByIdWithLockAsync(int id)
    {
        return await _context.Accounts
            .FromSqlRaw("SELECT * FROM Accounts WITH (UPDLOCK, HOLDLOCK) WHERE AccountId = {0}", id)
            .FirstOrDefaultAsync();
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> UpdateAsync(Account account)
    {
        account.LastModified = DateTime.UtcNow;
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return false;
        account.IsDeleted = true;
        account.IsActive = false;
        account.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Accounts.AnyAsync(a => a.AccountId == id);

    public async Task<decimal> GetTotalBalanceAsync() =>
        await _context.Accounts.Where(a => a.IsActive).SumAsync(a => a.Balance);
}

public class TransactionRepository : ITransactionRepository
{
    private readonly BankDbContext _context;

    public TransactionRepository(BankDbContext context) => _context = context;

    public async Task<Transaction?> GetByIdAsync(long id) =>
        await _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TransactionId == id);

    public async Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber) =>
        await _context.Transactions.FirstOrDefaultAsync(t => t.ReferenceNumber == referenceNumber);

    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetAllAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? type = null)
    {
        var query = _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.User)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.Type.ToString() == type);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetByAccountIdAsync(int accountId, int page, int pageSize)
    {
        var query = _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<Transaction>> GetRecentAsync(int count) =>
        await _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<decimal> GetTodayTotalByTypeAsync(string type)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        if (type == "Deposit")
        {
            return await _context.Transactions
                .Where(t => t.Type == TransactionType.Deposit && t.CreatedAt >= today && t.CreatedAt < tomorrow)
                .SumAsync(t => t.Amount);
        }
        else if (type == "Withdrawal")
        {
            return await _context.Transactions
                .Where(t => t.Type == TransactionType.Withdrawal && t.CreatedAt >= today && t.CreatedAt < tomorrow)
                .SumAsync(t => t.Amount);
        }
        
        return 0;
    }

    public async Task<decimal> GetTotalByTypeInRangeAsync(string type, DateTime start, DateTime end)
    {
        var query = _context.Transactions.Where(t => t.CreatedAt >= start && t.CreatedAt <= end);
        
        if (type == "Deposit")
            return await query.Where(t => t.Type == TransactionType.Deposit).SumAsync(t => t.Amount);
        else if (type == "Withdrawal")
            return await query.Where(t => t.Type == TransactionType.Withdrawal).SumAsync(t => t.Amount);
        
        return 0;
    }
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly BankDbContext _context;

    public AuditLogRepository(BankDbContext context) => _context = context;

    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? userId = null, string? action = null)
    {
        var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int count) =>
        await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
}
