using BankSystem.Api.DTOs.Requests;
using BankSystem.Api.DTOs.Responses;
using BankSystem.Core.Entities;
using BankSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public UsersController(IUserService userService, IAuditService auditService)
    {
        _userService = userService;
        _auditService = auditService;
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        var users = await _userService.GetAllAsync();
        var userResponses = users.Select(u => new UserResponse
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role.ToString(),
            LastLoginAt = u.LastLoginAt
        });

        return Ok(new PagedResponse<UserResponse>
        {
            Items = userResponses,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = userResponses.Count()
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(new ApiResponse<UserResponse> { Success = false, Message = "User not found" });

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Data = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                LastLoginAt = user.LastLoginAt
            }
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(
                request.Username,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                Enum.Parse<UserRole>(request.Role, true));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _auditService.LogAsync(userId, "CREATE", "User", user.UserId, null, user, null, null);

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User created successfully",
                Data = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString()
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<UserResponse> { Success = false, Message = ex.Message });
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}/role")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var role = Enum.Parse<UserRole>(request.Role, true);
            var result = await _userService.UpdateRoleAsync(id, role);

            if (!result)
                return NotFound(new ApiResponse<bool> { Success = false, Message = "User not found" });

            return Ok(new ApiResponse<bool> { Success = true, Message = "Role updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<bool> { Success = false, Message = ex.Message });
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result)
            return NotFound(new ApiResponse<bool> { Success = false, Message = "User not found" });

        return Ok(new ApiResponse<bool> { Success = true, Message = "User deleted successfully" });
    }
}
