using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using BankSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace BankSystem.Services.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly BankDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, BankDbContext context, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Token, string? RefreshToken, User? User, string? Error)> LoginAsync(string username, string password, string ipAddress)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null || !user.IsActive)
        {
            await LogAttempt(username, ipAddress, false, "User not found or inactive");
            return (false, null, null, null, "Invalid username or password");
        }

        if (!BC.Verify(password, user.PasswordHash))
        {
            await LogAttempt(username, ipAddress, false, "Invalid password");
            return (false, null, null, null, "Invalid username or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var token = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

        await LogAttempt(username, ipAddress, true, null);

        return (true, token, refreshToken, user, null);
    }

    public async Task<(bool Success, string? Token, string? Error)> RefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

        if (refreshToken == null || refreshToken.User == null || !refreshToken.User.IsActive)
            return (false, null, "Invalid refresh token");

        var newToken = GenerateJwtToken(refreshToken.User);
        return (true, newToken, null);
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        if (refreshToken == null) return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        var tokens = await _context.RefreshTokens.Where(r => r.UserId == userId && !r.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "BankSystem",
            audience: _configuration["Jwt:Audience"] ?? "BankSystemAdmin",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(int userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken.Token;
    }

    private async Task LogAttempt(string username, string ipAddress, bool success, string? failureReason)
    {
        var attempt = new LoginAttempt
        {
            Username = username,
            IPAddress = ipAddress,
            Success = success,
            FailureReason = failureReason,
            AttemptedAt = DateTime.UtcNow
        };

        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync();
    }
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IAuditService auditService)
    {
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<User?> GetByIdAsync(int id) => await _userRepository.GetByIdAsync(id);

    public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();

    public async Task<User> CreateAsync(string username, string email, string password, string firstName, string lastName, UserRole role)
    {
        if (await _userRepository.UsernameExistsAsync(username))
            throw new InvalidOperationException("Username already exists");
        if (await _userRepository.EmailExistsAsync(email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BC.HashPassword(password, 12),
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<bool> UpdateRoleAsync(int id, UserRole role)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        var oldRole = user.Role;
        user.Role = role;
        await _userRepository.UpdateAsync(user);

        await _auditService.LogAsync(0, "UPDATE_ROLE", "User", id, new { oldRole }, new { newRole = role }, null, null);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _userRepository.DeleteAsync(id);
        if (result)
            await _auditService.LogAsync(0, "DELETE", "User", id, null, null, null, null);
        return result;
    }

    public async Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !BC.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BC.HashPassword(newPassword, 12);
        await _userRepository.UpdateAsync(user);
        return true;
    }
}

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepository;

    public AuditService(IAuditLogRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task LogAsync(int userId, string action, string entityType, int? entityId, object? oldValues, object? newValues, string? ipAddress, string? userAgent)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.CreateAsync(auditLog);
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize, int? userId = null, string? action = null)
        => await _auditRepository.GetAllAsync(page, pageSize, userId, action);
}
