using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Services.Services;

public class DashboardService : IDashboardService
{
    private readonly BankDbContext _context;

    public DashboardService(BankDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        return new DashboardSummary
        {
            TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive),
            TotalAccounts = await _context.Accounts.CountAsync(a => a.IsActive),
            TotalBalance = await _context.Accounts.Where(a => a.IsActive).SumAsync(a => a.Balance),
            TodayDeposits = await _context.Transactions
                .Where(t => t.Type == TransactionType.Deposit && t.CreatedAt >= today && t.CreatedAt < tomorrow)
                .SumAsync(t => t.Amount),
            TodayWithdrawals = await _context.Transactions
                .Where(t => t.Type == TransactionType.Withdrawal && t.CreatedAt >= today && t.CreatedAt < tomorrow)
                .SumAsync(t => t.Amount),
            TodayTransactions = await _context.Transactions
                .CountAsync(t => t.CreatedAt >= today && t.CreatedAt < tomorrow),
            MonthlyRevenue = await _context.Transactions
                .Where(t => t.Type == TransactionType.Deposit && t.CreatedAt >= monthStart)
                .SumAsync(t => t.Amount)
        };
    }

    public async Task<ChartData> GetChartDataAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var transactionTrends = await _context.Transactions
            .Where(t => t.CreatedAt >= startDate)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new TransactionTrend
            {
                Date = g.Key,
                Deposits = g.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount),
                Withdrawals = g.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount),
                Transfers = g.Where(t => t.Type == TransactionType.Transfer).Sum(t => t.Amount)
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        var accountDistribution = await _context.Accounts
            .Where(a => a.IsActive)
            .GroupBy(a => a.AccountType)
            .Select(g => new AccountDistribution
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                TotalBalance = g.Sum(a => a.Balance)
            })
            .ToListAsync();

        var balanceTrends = new List<MonthlyBalance>();
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var balance = await _context.Transactions
                .Where(t => t.CreatedAt < monthEnd)
                .SumAsync(t => 
                    t.Type == TransactionType.Deposit ? t.Amount :
                    t.Type == TransactionType.Withdrawal ? -t.Amount : 0);

            balanceTrends.Add(new MonthlyBalance
            {
                Month = date.ToString("MMM yyyy"),
                Balance = balance
            });
        }

        return new ChartData
        {
            TransactionTrends = transactionTrends,
            AccountTypeDistribution = accountDistribution,
            BalanceTrends = balanceTrends
        };
    }
}
