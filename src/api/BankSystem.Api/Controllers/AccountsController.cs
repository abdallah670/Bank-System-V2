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
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;

    public AccountsController(IAccountService accountService, ICustomerService customerService, IAuditService auditService)
    {
        _accountService = accountService;
        _customerService = customerService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AccountResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        var (items, totalCount) = await _accountService.GetAllAsync(request.Page, request.PageSize, null);
        var accounts = items.Select(a => new AccountResponse
        {
            AccountId = a.AccountId,
            AccountNumber = a.AccountNumber,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer?.FullName ?? "",
            AccountType = a.AccountType.ToString(),
            Currency = a.Currency,
            Balance = a.Balance,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt
        });

        return Ok(new PagedResponse<AccountResponse>
        {
            Items = accounts,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null)
            return NotFound(new ApiResponse<AccountResponse> { Success = false, Message = "Account not found" });

        return Ok(new ApiResponse<AccountResponse>
        {
            Success = true,
            Data = new AccountResponse
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                CustomerName = account.Customer?.FullName ?? "",
                AccountType = account.AccountType.ToString(),
                Currency = account.Currency,
                Balance = account.Balance,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt
            }
        });
    }

    [HttpGet("by-number/{accountNumber}")]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> GetByAccountNumber(string accountNumber)
    {
        var account = await _accountService.GetByAccountNumberAsync(accountNumber);
        if (account == null)
            return NotFound(new ApiResponse<AccountResponse> { Success = false, Message = "Account not found" });

        return Ok(new ApiResponse<AccountResponse>
        {
            Success = true,
            Data = new AccountResponse
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                CustomerName = account.Customer?.FullName ?? "",
                AccountType = account.AccountType.ToString(),
                Currency = account.Currency,
                Balance = account.Balance,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt
            }
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> Create([FromBody] CreateAccountRequest request)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return BadRequest(new ApiResponse<AccountResponse> { Success = false, Message = "Customer not found" });

            var account = await _accountService.CreateAsync(request.CustomerId, request.AccountType, request.Currency, request.InitialBalance);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _auditService.LogAsync(userId, "CREATE", "Account", account.AccountId, null, account, null, null);

            return CreatedAtAction(nameof(GetById), new { id = account.AccountId }, new ApiResponse<AccountResponse>
            {
                Success = true,
                Message = "Account created successfully",
                Data = new AccountResponse
                {
                    AccountId = account.AccountId,
                    AccountNumber = account.AccountNumber,
                    CustomerId = account.CustomerId,
                    CustomerName = customer.FullName,
                    AccountType = account.AccountType.ToString(),
                    Currency = account.Currency,
                    Balance = account.Balance,
                    IsActive = account.IsActive,
                    CreatedAt = account.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<AccountResponse> { Success = false, Message = ex.Message });
        }
    }
}
