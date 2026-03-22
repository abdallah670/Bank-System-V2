using BankSystem.Api.DTOs.Requests;
using BankSystem.Api.DTOs.Responses;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SessionDto>>>> GetSessions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var sessions = await _sessionService.GetUserSessionsAsync(userId);

        return Ok(new ApiResponse<List<SessionDto>>
        {
            Success = true,
            Data = sessions.Select(s => new SessionDto
            {
                SessionId = s.SessionId,
                IPAddress = s.IPAddress ?? "Unknown",
                UserAgent = s.UserAgent ?? "Unknown",
                CreatedAt = s.CreatedAt,
                LastActivityAt = s.LastActivityAt ?? s.CreatedAt,
                IsCurrent = s.SessionId == Request.Headers["X-Session-Id"].FirstOrDefault()
            }).ToList()
        });
    }

    [HttpGet("active-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetActiveCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var count = await _sessionService.GetActiveSessionCountAsync(userId);

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = count
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<bool>>> RefreshSession()
    {
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(sessionId))
            return BadRequest(new ApiResponse<bool> { Success = false, Message = "Session ID required" });

        await _sessionService.RefreshSessionAsync(sessionId);
        return Ok(new ApiResponse<bool> { Success = true, Message = "Session refreshed" });
    }

    [HttpPost("revoke/{sessionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(string sessionId)
    {
        await _sessionService.RevokeSessionAsync(sessionId);
        return Ok(new ApiResponse<bool> { Success = true, Message = "Session revoked" });
    }

    [HttpPost("revoke-all")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeAllSessions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var exceptSessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
        
        await _sessionService.RevokeAllSessionsAsync(userId, exceptSessionId);
        return Ok(new ApiResponse<bool> { Success = true, Message = "All other sessions revoked" });
    }
}

public class SessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public bool IsCurrent { get; set; }
}
