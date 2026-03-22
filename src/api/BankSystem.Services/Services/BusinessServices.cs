using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;

namespace BankSystem.Services.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditService _auditService;

    public CustomerService(ICustomerRepository customerRepository, IAuditService auditService)
    {
        _customerRepository = customerRepository;
        _auditService = auditService;
    }

    public async Task<Customer?> GetByIdAsync(int id) => await _customerRepository.GetByIdAsync(id);

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
        => await _customerRepository.GetAllAsync(page, pageSize, search);

    public async Task<Customer> CreateAsync(string firstName, string lastName, string email, string phone, string? address, string? city, string country, DateTime? dateOfBirth)
    {
        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Address = address,
            City = city,
            Country = country,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var result = await _customerRepository.CreateAsync(customer);
        await _auditService.LogAsync(0, "CREATE", "Customer", result.CustomerId, null, customer, null, null);
        return result;
    }

    public async Task<Customer?> UpdateAsync(int id, string firstName, string lastName, string email, string phone, string? address, string? city, string country)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        var oldCustomer = new { customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.Address, customer.City, customer.Country };

        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = address;
        customer.City = city;
        customer.Country = country;

        var result = await _customerRepository.UpdateAsync(customer);
        await _auditService.LogAsync(0, "UPDATE", "Customer", id, oldCustomer, customer, null, null);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _customerRepository.DeleteAsync(id);
        if (result)
            await _auditService.LogAsync(0, "DELETE", "Customer", id, null, null, null, null);
        return result;
    }
}

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditService _auditService;

    public AccountService(IAccountRepository accountRepository, ICustomerRepository customerRepository, IAuditService auditService)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _auditService = auditService;
    }

    public async Task<Account?> GetByIdAsync(int id) => await _accountRepository.GetByIdAsync(id);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber) => await _accountRepository.GetByAccountNumberAsync(accountNumber);

    public async Task<(IEnumerable<Account> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? customerId = null)
        => await _accountRepository.GetAllAsync(page, pageSize, customerId);

    public async Task<decimal> GetTotalBalanceAsync() => await _accountRepository.GetTotalBalanceAsync();

    public async Task<Account> CreateAsync(int customerId, string accountType, string currency, decimal initialBalance)
    {
        if (!await _customerRepository.ExistsAsync(customerId))
            throw new InvalidOperationException("Customer not found");

        var accountNumber = GenerateAccountNumber();

        var account = new Account
        {
            AccountNumber = accountNumber,
            CustomerId = customerId,
            AccountType = Enum.Parse<AccountType>(accountType, true),
            Currency = currency,
            Balance = initialBalance,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var result = await _accountRepository.CreateAsync(account);
        await _auditService.LogAsync(0, "CREATE", "Account", result.AccountId, null, account, null, null);
        return result;
    }

    private string GenerateAccountNumber()
    {
        return DateTime.UtcNow.ToString("yyMMdd") + Random.Shared.Next(100000, 999999);
    }
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAuditService _auditService;
    private readonly BankDbContext _context;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAuditService auditService,
        BankDbContext context)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _auditService = auditService;
        _context = context;
    }

    public async Task<Transaction> DepositAsync(int accountId, decimal amount, string? description, int userId)
    {
        var account = await _accountRepository.GetByIdWithLockAsync(accountId);
        if (account == null || !account.IsActive)
            throw new InvalidOperationException("Account not found or inactive");

        account.Balance += amount;
        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction
        {
            Type = TransactionType.Deposit,
            ToAccountId = accountId,
            Amount = amount,
            BalanceAfter = account.Balance,
            ReferenceNumber = GenerateReference("DEP"),
            Description = description,
            Status = TransactionStatus.Completed,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _transactionRepository.CreateAsync(transaction);
        await _auditService.LogAsync(userId, "DEPOSIT", "Transaction", result.TransactionId, null, transaction, null, null);
        return result;
    }

    public async Task<Transaction> WithdrawAsync(int accountId, decimal amount, string? description, int userId)
    {
        var account = await _accountRepository.GetByIdWithLockAsync(accountId);
        if (account == null || !account.IsActive)
            throw new InvalidOperationException("Account not found or inactive");

        if (account.Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        account.Balance -= amount;
        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction
        {
            Type = TransactionType.Withdrawal,
            FromAccountId = accountId,
            Amount = amount,
            BalanceAfter = account.Balance,
            ReferenceNumber = GenerateReference("WDL"),
            Description = description,
            Status = TransactionStatus.Completed,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _transactionRepository.CreateAsync(transaction);
        await _auditService.LogAsync(userId, "WITHDRAWAL", "Transaction", result.TransactionId, null, transaction, null, null);
        return result;
    }

    public async Task<(Transaction? FromTransaction, Transaction? ToTransaction)> TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? description, int userId)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var fromAccount = await _accountRepository.GetByIdWithLockAsync(fromAccountId);
            var toAccount = await _accountRepository.GetByIdWithLockAsync(toAccountId);

            if (fromAccount == null || !fromAccount.IsActive)
                throw new InvalidOperationException("Source account not found or inactive");

            if (toAccount == null || !toAccount.IsActive)
                throw new InvalidOperationException("Destination account not found or inactive");

            if (fromAccount.Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            await _accountRepository.UpdateAsync(fromAccount);
            await _accountRepository.UpdateAsync(toAccount);

            var referenceNumber = GenerateReference("TRF");

            var fromTransaction = new Transaction
            {
                Type = TransactionType.Transfer,
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount,
                BalanceAfter = fromAccount.Balance,
                ReferenceNumber = referenceNumber,
                Description = description ?? $"Transfer to {toAccount.AccountNumber}",
                Status = TransactionStatus.Completed,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            var toTransaction = new Transaction
            {
                Type = TransactionType.Transfer,
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount,
                BalanceAfter = toAccount.Balance,
                ReferenceNumber = referenceNumber + "-R",
                Description = description ?? $"Transfer from {fromAccount.AccountNumber}",
                Status = TransactionStatus.Completed,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.CreateAsync(fromTransaction);
            await _transactionRepository.CreateAsync(toTransaction);

            await dbTransaction.CommitAsync();

            await _auditService.LogAsync(userId, "TRANSFER", "Transaction", fromTransaction.TransactionId, null, 
                new { FromAccount = fromAccountId, ToAccount = toAccountId, Amount = amount }, null, null, null);

            return (fromTransaction, toTransaction);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Transaction?> GetByIdAsync(long id) => await _transactionRepository.GetByIdAsync(id);

    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetAllAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? type = null)
        => await _transactionRepository.GetAllAsync(page, pageSize, startDate, endDate, type);

    public async Task<IEnumerable<Transaction>> GetRecentAsync(int count) => await _transactionRepository.GetRecentAsync(count);

    private string GenerateReference(string prefix)
    {
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
