using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BankSystem.Services.Services;

public class SessionService : ISessionService
{
    private readonly BankDbContext _context;
    private const int SessionTimeoutMinutes = 60;

    public SessionService(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Session> CreateSessionAsync(int userId, string? ipAddress, string? userAgent)
    {
        var sessionId = GenerateSessionId();
        
        var session = new Session
        {
            UserId = userId,
            SessionId = sessionId,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(SessionTimeoutMinutes),
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return null;

        if (session.IsRevoked)
            return null;

        if (session.ExpiresAt < DateTime.UtcNow)
            return null;

        return session;
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return session != null;
    }

    public async Task RefreshSessionAsync(string sessionId)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session != null && !session.IsRevoked)
        {
            session.LastActivityAt = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(SessionTimeoutMinutes);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeSessionAsync(string sessionId)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session != null)
        {
            session.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllSessionsAsync(int userId, string? exceptSessionId = null)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.SessionId != exceptSessionId)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Session>> GetUserSessionsAsync(int userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync();
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await _context.Sessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || s.IsRevoked)
            .ToListAsync();

        _context.Sessions.RemoveRange(expiredSessions);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetActiveSessionCountAsync(int userId)
    {
        return await _context.Sessions
            .CountAsync(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
    }

    private string GenerateSessionId()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

public interface ISessionService
{
    Task<Session> CreateSessionAsync(int userId, string? ipAddress, string? userAgent);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<bool> ValidateSessionAsync(string sessionId);
    Task RefreshSessionAsync(string sessionId);
    Task RevokeSessionAsync(string sessionId);
    Task RevokeAllSessionsAsync(int userId, string? exceptSessionId = null);
    Task<IEnumerable<Session>> GetUserSessionsAsync(int userId);
    Task CleanupExpiredSessionsAsync();
    Task<int> GetActiveSessionCountAsync(int userId);
}
