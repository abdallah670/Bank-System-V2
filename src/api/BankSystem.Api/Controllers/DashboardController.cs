using BankSystem.Api.DTOs.Requests;
using BankSystem.Api.DTOs.Responses;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummary>>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(new ApiResponse<DashboardSummary>
        {
            Success = true,
            Data = summary
        });
    }

    [HttpGet("chart-data")]
    public async Task<ActionResult<ApiResponse<ChartData>>> GetChartData([FromQuery] int days = 30)
    {
        var chartData = await _dashboardService.GetChartDataAsync(days);
        return Ok(new ApiResponse<ChartData>
        {
            Success = true,
            Data = chartData
        });
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> GetLogs([FromQuery] PaginationRequest request)
    {
        var (items, totalCount) = await _auditService.GetAllAsync(request.Page, request.PageSize, null, request.Type);

        var logs = items.Select(l => new AuditLogResponse
        {
            AuditId = l.AuditId,
            UserId = l.UserId,
            Username = l.User?.Username ?? "",
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            IPAddress = l.IPAddress,
            Timestamp = l.Timestamp
        });

        return Ok(new PagedResponse<AuditLogResponse>
        {
            Items = logs,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
