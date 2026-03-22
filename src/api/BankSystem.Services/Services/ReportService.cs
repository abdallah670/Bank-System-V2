using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace BankSystem.Services.Services;

public class ReportService : IReportService
{
    private readonly BankDbContext _context;

    public ReportService(BankDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateTransactionsReportAsync(DateTime startDate, DateTime endDate, string? accountNumber = null)
    {
        var query = _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.User)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(accountNumber))
        {
            query = query.Where(t => 
                t.FromAccount!.AccountNumber == accountNumber || 
                t.ToAccount!.AccountNumber == accountNumber);
        }

        var transactions = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 12;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 15;
        worksheet.Column(7).Width = 30;
        worksheet.Column(8).Width = 15;

        var headerRow = worksheet.Range("A1:H1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(1, 1).Value = "Date";
        worksheet.Cell(1, 2).Value = "Type";
        worksheet.Cell(1, 3).Value = "From Account";
        worksheet.Cell(1, 4).Value = "To Account";
        worksheet.Cell(1, 5).Value = "Amount";
        worksheet.Cell(1, 6).Value = "Balance After";
        worksheet.Cell(1, 7).Value = "Description";
        worksheet.Cell(1, 8).Value = "Reference";

        int row = 2;
        foreach (var t in transactions)
        {
            worksheet.Cell(row, 1).Value = t.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(row, 2).Value = t.Type.ToString();
            worksheet.Cell(row, 3).Value = t.FromAccount?.AccountNumber ?? "-";
            worksheet.Cell(row, 4).Value = t.ToAccount?.AccountNumber ?? "-";
            worksheet.Cell(row, 5).Value = t.Amount;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 6).Value = t.BalanceAfter;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 7).Value = t.Description ?? "";
            worksheet.Cell(row, 8).Value = t.ReferenceNumber;
            row++;
        }

        worksheet.Cell(row + 1, 1).Value = $"Total Transactions: {transactions.Count}";
        worksheet.Cell(row + 2, 1).Value = $"Total Deposits: {transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount):C}";
        worksheet.Cell(row + 3, 1).Value = $"Total Withdrawals: {transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount):C}";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateCustomersReportAsync()
    {
        var customers = await _context.Customers
            .Include(c => c.Accounts)
            .Where(c => c.IsActive)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Customers");

        worksheet.Column(1).Width = 8;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 15;

        var headerRow = worksheet.Range("A1:F1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "First Name";
        worksheet.Cell(1, 3).Value = "Last Name";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Phone";
        worksheet.Cell(1, 6).Value = "Accounts";

        int row = 2;
        foreach (var c in customers)
        {
            worksheet.Cell(row, 1).Value = c.CustomerId;
            worksheet.Cell(row, 2).Value = c.FirstName;
            worksheet.Cell(row, 3).Value = c.LastName;
            worksheet.Cell(row, 4).Value = c.Email;
            worksheet.Cell(row, 5).Value = c.Phone;
            worksheet.Cell(row, 6).Value = c.Accounts.Count;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateAccountsReportAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.Customer)
            .Where(a => a.IsActive)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Accounts");

        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).width = 15;
        worksheet.Column(5).width = 15;

        var headerRow = worksheet.Range("A1:E1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(1, 1).Value = "Account Number";
        worksheet.Cell(1, 2).Value = "Customer";
        worksheet.Cell(1, 3).Value = "Type";
        worksheet.Cell(1, 4).Value = "Currency";
        worksheet.Cell(1, 5).Value = "Balance";

        int row = 2;
        decimal totalBalance = 0;
        foreach (var a in accounts)
        {
            worksheet.Cell(row, 1).Value = a.AccountNumber;
            worksheet.Cell(row, 2).Value = a.Customer?.FullName ?? "N/A";
            worksheet.Cell(row, 3).Value = a.AccountType.ToString();
            worksheet.Cell(row, 4).Value = a.Currency;
            worksheet.Cell(row, 5).Value = a.Balance;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            totalBalance += a.Balance;
            row++;
        }

        worksheet.Cell(row + 1, 1).Value = $"Total Accounts: {accounts.Count}";
        worksheet.Cell(row + 2, 1).Value = $"Total Balance: {totalBalance:C}";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateAuditLogReportAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Audit Logs");

        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 15;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 10;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 15;

        var headerRow = worksheet.Range("A1:F1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(1, 1).Value = "Timestamp";
        worksheet.Cell(1, 2).Value = "Username";
        worksheet.Cell(1, 3).Value = "Action";
        worksheet.Cell(1, 4).Value = "Entity";
        worksheet.Cell(1, 5).Value = "Entity ID";
        worksheet.Cell(1, 6).Value = "IP Address";

        int row = 2;
        foreach (var log in logs)
        {
            worksheet.Cell(row, 1).Value = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 2).Value = log.User?.Username ?? "N/A";
            worksheet.Cell(row, 3).Value = log.Action;
            worksheet.Cell(row, 4).Value = log.EntityType;
            worksheet.Cell(row, 5).Value = log.EntityId?.ToString() ?? "-";
            worksheet.Cell(row, 6).Value = log.IPAddress ?? "-";
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<DashboardSummary> GetAdvancedDashboardAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var transactions = await _context.Transactions
            .Where(t => t.CreatedAt >= start && t.CreatedAt <= end)
            .ToListAsync();

        return new DashboardSummary
        {
            TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive),
            TotalAccounts = await _context.Accounts.CountAsync(a => a.IsActive),
            TotalBalance = await _context.Accounts.Where(a => a.IsActive).SumAsync(a => a.Balance),
            TodayDeposits = transactions.Where(t => t.Type == TransactionType.Deposit && t.CreatedAt.Date == DateTime.UtcNow.Date).Sum(t => t.Amount),
            TodayWithdrawals = transactions.Where(t => t.Type == TransactionType.Withdrawal && t.CreatedAt.Date == DateTime.UtcNow.Date).Sum(t => t.Amount),
            TodayTransactions = transactions.Count(t => t.CreatedAt.Date == DateTime.UtcNow.Date),
            MonthlyRevenue = transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount)
        };
    }
}

public interface IReportService
{
    Task<byte[]> GenerateTransactionsReportAsync(DateTime startDate, DateTime endDate, string? accountNumber = null);
    Task<byte[]> GenerateCustomersReportAsync();
    Task<byte[]> GenerateAccountsReportAsync();
    Task<byte[]> GenerateAuditLogReportAsync(DateTime startDate, DateTime endDate);
    Task<DashboardSummary> GetAdvancedDashboardAsync(DateTime? startDate = null, DateTime? endDate = null);
}
