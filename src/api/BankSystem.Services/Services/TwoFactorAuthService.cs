using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace BankSystem.Services.Services;

public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly BankDbContext _context;

    public TwoFactorAuthService(BankDbContext context)
    {
        _context = context;
    }

    public async Task<(string SecretKey, string QRCodeUrl)> GenerateSecretKeyAsync(int userId)
    {
        var secretKey = Base32Encoding.ToString(RandomNumberGenerator.GetBytes(20));
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");

        var issuer = "BankSystemV2";
        var accountName = user.Email;
        var otpUri = $"otpauth://totp/{issuer}:{accountName}?secret={secretKey}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

        return (secretKey, otpUri);
    }

    public async Task<bool> EnableTwoFactorAsync(int userId, string secretKey, string verificationCode)
    {
        if (!VerifyCode(secretKey, verificationCode))
            return false;

        var existing = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        
        if (existing != null)
        {
            existing.SecretKey = secretKey;
            existing.IsEnabled = true;
            existing.IsVerified = true;
        }
        else
        {
            _context.TwoFactorAuths.Add(new TwoFactorAuth
            {
                UserId = userId,
                SecretKey = secretKey,
                IsEnabled = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableTwoFactorAsync(int userId)
    {
        var twoFactor = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        if (twoFactor == null) return false;

        twoFactor.IsEnabled = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsTwoFactorEnabledAsync(int userId)
    {
        var twoFactor = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        return twoFactor?.IsEnabled ?? false;
    }

    public async Task<TwoFactorAuth?> GetTwoFactorAuthAsync(int userId)
    {
        return await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public bool VerifyCode(string secretKey, string code)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes);
            return totp.Verify(code);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GenerateBackupCodesAsync(int userId)
    {
        var codes = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            codes.Add(RandomNumberGenerator.GetHexString(16));
        }

        var twoFactor = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        if (twoFactor != null)
        {
            twoFactor.BackupCodes = string.Join(",", codes);
            await _context.SaveChangesAsync();
        }

        return string.Join("\n", codes);
    }

    public async Task<bool> VerifyBackupCodeAsync(int userId, string code)
    {
        var twoFactor = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        if (twoFactor == null || string.IsNullOrEmpty(twoFactor.BackupCodes))
            return false;

        var codes = twoFactor.BackupCodes.Split(',').ToList();
        var codeIndex = codes.FindIndex(c => c.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (codeIndex >= 0)
        {
            codes.RemoveAt(codeIndex);
            twoFactor.BackupCodes = string.Join(",", codes);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}

public interface ITwoFactorAuthService
{
    Task<(string SecretKey, string QRCodeUrl)> GenerateSecretKeyAsync(int userId);
    Task<bool> EnableTwoFactorAsync(int userId, string secretKey, string verificationCode);
    Task<bool> DisableTwoFactorAsync(int userId);
    Task<bool> IsTwoFactorEnabledAsync(int userId);
    Task<TwoFactorAuth?> GetTwoFactorAuthAsync(int userId);
    bool VerifyCode(string secretKey, string code);
    Task<string> GenerateBackupCodesAsync(int userId);
    Task<bool> VerifyBackupCodeAsync(int userId, string code);
}
