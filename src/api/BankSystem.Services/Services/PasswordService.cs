using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace BankSystem.Services.Services;

public class PasswordService : IPasswordService
{
    private readonly BankDbContext _context;
    private const int PasswordHistoryCount = 5;
    private const int MinPasswordLength = 8;

    public PasswordService(BankDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ValidatePasswordAsync(int userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        return BC.Verify(password, user.PasswordHash);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (!BC.Verify(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        var validation = ValidatePasswordPolicy(newPassword);
        if (!validation.Success)
            return (false, validation.Error);

        if (await IsPasswordInHistoryAsync(userId, newPassword))
            return (false, "Password was used recently. Please choose a different password.");

        await AddToPasswordHistoryAsync(userId, user.PasswordHash);

        user.PasswordHash = BC.HashPassword(newPassword, 12);
        user.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        var validation = ValidatePasswordPolicy(newPassword);
        if (!validation.Success)
            return (false, validation.Error);

        await AddToPasswordHistoryAsync(userId, user.PasswordHash);

        user.PasswordHash = BC.HashPassword(newPassword, 12);
        user.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public (bool Success, string? Error) ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");

        if (password.Length < MinPasswordLength)
            return (false, $"Password must be at least {MinPasswordLength} characters long");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one number");

        if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
            return (false, "Password must contain at least one special character");

        return (true, null);
    }

    private async Task<bool> IsPasswordInHistoryAsync(int userId, string newPassword)
    {
        var history = await _context.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(PasswordHistoryCount)
            .ToListAsync();

        foreach (var entry in history)
        {
            if (BC.Verify(newPassword, entry.PasswordHash))
                return true;
        }

        return false;
    }

    private async Task AddToPasswordHistoryAsync(int userId, string passwordHash)
    {
        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        });

        var oldPasswords = await _context.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(PasswordHistoryCount)
            .ToListAsync();

        _context.PasswordHistories.RemoveRange(oldPasswords);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PasswordHistory>> GetPasswordHistoryAsync(int userId)
    {
        return await _context.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}

public interface IPasswordService
{
    Task<bool> ValidatePasswordAsync(int userId, string password);
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<(bool Success, string? Error)> ResetPasswordAsync(int userId, string newPassword);
    (bool Success, string? Error) ValidatePasswordPolicy(string password);
    Task<IEnumerable<PasswordHistory>> GetPasswordHistoryAsync(int userId);
}
