using BankSystem.Api.DTOs.Requests;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactionsReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? accountNumber = null)
    {
        var report = await _reportService.GenerateTransactionsReportAsync(startDate, endDate, accountNumber);
        return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"transactions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomersReport()
    {
        var report = await _reportService.GenerateCustomersReportAsync();
        return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"customers_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccountsReport()
    {
        var report = await _reportService.GenerateAccountsReportAsync();
        return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"accounts_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _reportService.GenerateAuditLogReportAsync(startDate, endDate);
        return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"audit_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardSummary>>> GetDashboardReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var report = await _reportService.GetAdvancedDashboardAsync(startDate, endDate);
        return Ok(new ApiResponse<DashboardSummary>
        {
            Success = true,
            Data = report
        });
    }
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

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
