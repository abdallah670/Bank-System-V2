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
public class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorAuthService _twoFactorService;
    private readonly IAuditService _auditService;

    public TwoFactorController(ITwoFactorAuthService twoFactorService, IAuditService auditService)
    {
        _twoFactorService = twoFactorService;
        _auditService = auditService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<TwoFactorStatusResponse>>> GetStatus()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var twoFactor = await _twoFactorService.GetTwoFactorAuthAsync(userId);

        return Ok(new ApiResponse<TwoFactorStatusResponse>
        {
            Success = true,
            Data = new TwoFactorStatusResponse
            {
                IsEnabled = twoFactor?.IsEnabled ?? false,
                IsVerified = twoFactor?.IsVerified ?? false
            }
        });
    }

    [HttpPost("setup")]
    public async Task<ActionResult<ApiResponse<TwoFactorSetupResponse>>> Setup()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var (secretKey, qrCodeUrl) = await _twoFactorService.GenerateSecretKeyAsync(userId);

        return Ok(new ApiResponse<TwoFactorSetupResponse>
        {
            Success = true,
            Data = new TwoFactorSetupResponse
            {
                SecretKey = secretKey,
                QRCodeUrl = qrCodeUrl
            }
        });
    }

    [HttpPost("enable")]
    public async Task<ActionResult<ApiResponse<bool>>> Enable([FromBody] TwoFactorVerifyRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var isValid = await _twoFactorService.EnableTwoFactorAsync(userId, request.SecretKey, request.Code);
        
        if (!isValid)
        {
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Invalid verification code"
            });
        }

        var backupCodes = await _twoFactorService.GenerateBackupCodesAsync(userId);

        var userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _auditService.LogAsync(userIdClaim, "ENABLE_2FA", "User", userId, null, new { Enabled = true }, null, null);

        return Ok(new ApiResponse<TwoFactorEnableResponse>
        {
            Success = true,
            Message = "Two-factor authentication enabled successfully",
            Data = new TwoFactorEnableResponse
            {
                BackupCodes = backupCodes.Split('\n').ToList()
            }
        });
    }

    [HttpPost("disable")]
    public async Task<ActionResult<ApiResponse<bool>>> Disable([FromBody] TwoFactorVerifyRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var twoFactor = await _twoFactorService.GetTwoFactorAuthAsync(userId);

        if (twoFactor == null)
        {
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Two-factor authentication is not enabled"
            });
        }

        if (!_twoFactorService.VerifyCode(twoFactor.SecretKey, request.Code))
        {
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Invalid verification code"
            });
        }

        await _twoFactorService.DisableTwoFactorAsync(userId);

        var userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _auditService.LogAsync(userIdClaim, "DISABLE_2FA", "User", userId, null, new { Disabled = true }, null, null);

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Two-factor authentication disabled successfully"
        });
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<bool>>> Verify([FromBody] TwoFactorVerifyRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var twoFactor = await _twoFactorService.GetTwoFactorAuthAsync(userId);

        if (twoFactor == null || !twoFactor.IsEnabled)
        {
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true
            });
        }

        var isValid = _twoFactorService.VerifyCode(twoFactor.SecretKey, request.Code);

        if (!isValid && !string.IsNullOrEmpty(twoFactor.BackupCodes))
        {
            isValid = await _twoFactorService.VerifyBackupCodeAsync(userId, request.Code);
        }

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = isValid
        });
    }
}

public class TwoFactorStatusResponse
{
    public bool IsEnabled { get; set; }
    public bool IsVerified { get; set; }
}

public class TwoFactorSetupResponse
{
    public string SecretKey { get; set; } = string.Empty;
    public string QRCodeUrl { get; set; } = string.Empty;
}

public class TwoFactorEnableResponse
{
    public List<string> BackupCodes { get; set; } = new();
}

public class TwoFactorVerifyRequest
{
    public string? SecretKey { get; set; }
    public string Code { get; set; } = string.Empty;
}
