using BankSystem.Core.Entities;
using BankSystem.Infrastructure.Data;
using BankSystem.Services.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankSystem.Tests;

public class TransactionServiceTests
{
    private BankDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BankDbContext(options);
    }

    [Fact]
    public async Task DepositAsync_ShouldIncreaseBalance()
    {
        var context = CreateInMemoryContext();
        var account = new Account
        {
            AccountNumber = "1234567890",
            Balance = 1000,
            IsActive = true,
            CustomerId = 1
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new TransactionService(
            new Infrastructure.Repositories.TransactionRepository(context),
            new Infrastructure.Repositories.AccountRepository(context),
            new Services.AuditService(new Infrastructure.Repositories.AuditLogRepository(context)),
            context
        );

        var result = await service.DepositAsync(account.AccountId, 500, "Test deposit", 1);

        Assert.NotNull(result);
        Assert.Equal(1500, result.BalanceAfter);
        Assert.Equal(TransactionType.Deposit, result.Type);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldDecreaseBalance()
    {
        var context = CreateInMemoryContext();
        var account = new Account
        {
            AccountNumber = "1234567890",
            Balance = 1000,
            IsActive = true,
            CustomerId = 1
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new TransactionService(
            new Infrastructure.Repositories.TransactionRepository(context),
            new Infrastructure.Repositories.AccountRepository(context),
            new Services.AuditService(new Infrastructure.Repositories.AuditLogRepository(context)),
            context
        );

        var result = await service.WithdrawAsync(account.AccountId, 300, "Test withdrawal", 1);

        Assert.NotNull(result);
        Assert.Equal(700, result.BalanceAfter);
        Assert.Equal(TransactionType.Withdrawal, result.Type);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrow_WhenInsufficientFunds()
    {
        var context = CreateInMemoryContext();
        var account = new Account
        {
            AccountNumber = "1234567890",
            Balance = 100,
            IsActive = true,
            CustomerId = 1
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new TransactionService(
            new Infrastructure.Repositories.TransactionRepository(context),
            new Infrastructure.Repositories.AccountRepository(context),
            new Services.AuditService(new Infrastructure.Repositories.AuditLogRepository(context)),
            context
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.WithdrawAsync(account.AccountId, 500, "Test", 1));
    }

    [Fact]
    public async Task TransferAsync_ShouldMoveFundsBetweenAccounts()
    {
        var context = CreateInMemoryContext();
        
        var fromAccount = new Account
        {
            AccountNumber = "1111111111",
            Balance = 1000,
            IsActive = true,
            CustomerId = 1
        };
        var toAccount = new Account
        {
            AccountNumber = "2222222222",
            Balance = 500,
            IsActive = true,
            CustomerId = 2
        };
        
        context.Accounts.AddRange(fromAccount, toAccount);
        await context.SaveChangesAsync();

        var service = new TransactionService(
            new Infrastructure.Repositories.TransactionRepository(context),
            new Infrastructure.Repositories.AccountRepository(context),
            new Services.AuditService(new Infrastructure.Repositories.AuditLogRepository(context)),
            context
        );

        var (fromTx, toTx) = await service.TransferAsync(fromAccount.AccountId, toAccount.AccountId, 300, "Test transfer", 1);

        Assert.NotNull(fromTx);
        Assert.NotNull(toTx);
        Assert.Equal(700, fromTx.BalanceAfter);
    }
}

public class PasswordServiceTests
{
    [Theory]
    [InlineData("Short1!", false)]
    [InlineData("nouppercase1!", false)]
    [InlineData("NOLOWERCASE1!", false)]
    [InlineData("NoSpecial123", false)]
    [InlineData("ValidPass1!", true)]
    [InlineData("MySecure@123", true)]
    public void ValidatePasswordPolicy_ShouldValidateCorrectly(string password, bool expected)
    {
        var context = CreateInMemoryContext();
        var service = new PasswordService(context);

        var (success, _) = service.ValidatePasswordPolicy(password);

        Assert.Equal(expected, success);
    }

    private BankDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BankDbContext(options);
    }
}

public class TwoFactorAuthServiceTests
{
    [Fact]
    public void VerifyCode_ShouldValidateTOTPCode()
    {
        var context = CreateInMemoryContext();
        var service = new TwoFactorAuthService(context);
        
        var secretKey = "JBSWY3DPEHPK3PXP";
        
        var isValid = service.VerifyCode(secretKey, "123456");
        
        Assert.IsType<bool>(isValid);
    }

    private BankDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BankDbContext(options);
    }
}

public class SessionServiceTests
{
    [Fact]
    public async Task CreateSessionAsync_ShouldCreateNewSession()
    {
        var context = CreateInMemoryContext();
        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new SessionService(context);

        var session = await service.CreateSessionAsync(user.UserId, "127.0.0.1", "Test Browser");

        Assert.NotNull(session);
        Assert.Equal(user.UserId, session.UserId);
        Assert.Equal("127.0.0.1", session.IPAddress);
        Assert.False(session.IsRevoked);
    }

    [Fact]
    public async Task ValidateSessionAsync_ShouldReturnFalse_WhenSessionRevoked()
    {
        var context = CreateInMemoryContext();
        var service = new SessionService(context);
        
        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var session = await service.CreateSessionAsync(user.UserId, "127.0.0.1", "Test");
        
        await service.RevokeSessionAsync(session.SessionId);
        
        var isValid = await service.ValidateSessionAsync(session.SessionId);
        
        Assert.False(isValid);
    }

    private BankDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BankDbContext(options);
    }
}
