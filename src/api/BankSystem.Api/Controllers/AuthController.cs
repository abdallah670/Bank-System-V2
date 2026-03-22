using BankSystem.Api.DTOs.Requests;
using BankSystem.Api.DTOs.Responses;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.LoginAsync(request.Username, request.Password, ipAddress);

        if (!result.Success || result.User == null)
        {
            return Unauthorized(new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new LoginResponse
            {
                Token = result.Token!,
                RefreshToken = result.RefreshToken!,
                User = new UserResponse
                {
                    UserId = result.User.UserId,
                    Username = result.User.Username,
                    Email = result.User.Email,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    Role = result.User.Role.ToString(),
                    LastLoginAt = result.User.LastLoginAt
                }
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token);

        if (!result.Success)
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Token refreshed",
            Data = result.Token
        });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeTokenAsync(request.Token);
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Logged out successfully"
        });
    }
}
