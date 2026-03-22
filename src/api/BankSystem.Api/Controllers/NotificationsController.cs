using BankSystem.Api.DTOs.Responses;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications([FromQuery] int count = 20)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, count);

        return Ok(new ApiResponse<List<NotificationDto>>
        {
            Success = true,
            Data = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                Link = n.Link,
                CreatedAt = n.CreatedAt
            }).ToList()
        });
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var count = await _notificationService.GetUnreadCountAsync(userId);

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = count
        });
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Notification marked as read"
        });
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "All notifications marked as read"
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        await _notificationService.DeleteAsync(id);
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Notification deleted"
        });
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; }
}
