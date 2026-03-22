using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Services.Services;

public class NotificationService : INotificationService
{
    private readonly BankDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(BankDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Notification> CreateAsync(int userId, string title, string message, NotificationType type, string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            await _emailService.SendEmailAsync(user.Email, title, message);
        }

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int count = 20)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task NotifyTransactionAsync(int userId, string transactionType, decimal amount, string accountNumber)
    {
        var title = $"{transactionType} Alert";
        var message = $"A {transactionType.ToLower()} of {amount:C} was processed on account {accountNumber}.";
        await CreateAsync(userId, title, message, NotificationType.Info);
    }

    public async Task NotifyLoginAsync(int userId, string ipAddress, bool success)
    {
        var title = success ? "New Login Detected" : "Failed Login Attempt";
        var message = success 
            ? $"A successful login was detected from IP: {ipAddress}"
            : $"A failed login attempt was detected from IP: {ipAddress}";
        var type = success ? NotificationType.Security : NotificationType.Warning;
        await CreateAsync(userId, title, message, type);
    }

    public async Task NotifyPasswordChangeAsync(int userId)
    {
        await CreateAsync(userId, "Password Changed", 
            "Your password was recently changed. If you did not make this change, please contact support immediately.",
            NotificationType.Security);
    }

    public async Task NotifyTwoFactorChangeAsync(int userId, bool enabled)
    {
        var title = enabled ? "Two-Factor Enabled" : "Two-Factor Disabled";
        var message = enabled 
            ? "Two-factor authentication has been enabled on your account."
            : "Two-factor authentication has been disabled on your account.";
        await CreateAsync(userId, title, message, NotificationType.Security);
    }
}

public interface INotificationService
{
    Task<Notification> CreateAsync(int userId, string title, string message, NotificationType type, string? link = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int count = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteAsync(int notificationId);
    Task NotifyTransactionAsync(int userId, string transactionType, decimal amount, string accountNumber);
    Task NotifyLoginAsync(int userId, string ipAddress, bool success);
    Task NotifyPasswordChangeAsync(int userId);
    Task NotifyTwoFactorChangeAsync(int userId, bool enabled);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailWithTemplateAsync(string to, string template, Dictionary<string, string> placeholders);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = _configuration["Email:SmtpPort"];
        var smtpUser = _configuration["Email:Username"];
        var smtpPassword = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromAddress"];

        if (string.IsNullOrEmpty(smtpHost))
        {
            Console.WriteLine($"[EMAIL] To: {to}, Subject: {subject}");
            Console.WriteLine($"[EMAIL] Body: {body}");
            return;
        }

        try
        {
            using var client = new System.Net.Mail.SmtpClient(smtpHost, int.Parse(smtpPort ?? "587"));
            client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
            client.EnableSsl = true;

            var message = new System.Net.Mail.MailMessage(fromEmail ?? "noreply@banksystem.com", to, subject, body);
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
        }
    }

    public async Task SendEmailWithTemplateAsync(string to, string template, Dictionary<string, string> placeholders)
    {
        var body = template;
        foreach (var placeholder in placeholders)
        {
            body = body.Replace($"{{{placeholder.Key}}}", placeholder.Value);
        }
        await SendEmailAsync(to, "BankSystem Notification", body);
    }
}
