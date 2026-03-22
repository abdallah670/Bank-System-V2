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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;

    public CustomersController(ICustomerService customerService, IAuditService auditService)
    {
        _customerService = customerService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        var (items, totalCount) = await _customerService.GetAllAsync(request.Page, request.PageSize, request.Search);
        var customers = items.Select(c => new CustomerResponse
        {
            CustomerId = c.CustomerId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            City = c.City,
            Country = c.Country,
            DateOfBirth = c.DateOfBirth,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            Accounts = c.Accounts?.Select(a => new AccountResponse
            {
                AccountId = a.AccountId,
                AccountNumber = a.AccountNumber,
                CustomerId = a.CustomerId,
                AccountType = a.AccountType.ToString(),
                Currency = a.Currency,
                Balance = a.Balance,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            }).ToList() ?? new List<AccountResponse>()
        });

        return Ok(new PagedResponse<CustomerResponse>
        {
            Items = customers,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new ApiResponse<CustomerResponse> { Success = false, Message = "Customer not found" });

        return Ok(new ApiResponse<CustomerResponse>
        {
            Success = true,
            Data = new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                DateOfBirth = customer.DateOfBirth,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                Accounts = customer.Accounts?.Select(a => new AccountResponse
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    CustomerId = a.CustomerId,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    AccountType = a.AccountType.ToString(),
                    Currency = a.Currency,
                    Balance = a.Balance,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<AccountResponse>()
            }
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Create([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.CreateAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.Address,
                request.City,
                request.Country,
                request.DateOfBirth);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _auditService.LogAsync(userId, "CREATE", "Customer", customer.CustomerId, null, customer, null, null);

            return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, new ApiResponse<CustomerResponse>
            {
                Success = true,
                Message = "Customer created successfully",
                Data = new CustomerResponse
                {
                    CustomerId = customer.CustomerId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    City = customer.City,
                    Country = customer.Country,
                    DateOfBirth = customer.DateOfBirth,
                    IsActive = customer.IsActive,
                    CreatedAt = customer.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<CustomerResponse> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = await _customerService.UpdateAsync(id, request.FirstName, request.LastName, request.Email, request.Phone, request.Address, request.City, request.Country);
        if (customer == null)
            return NotFound(new ApiResponse<CustomerResponse> { Success = false, Message = "Customer not found" });

        return Ok(new ApiResponse<CustomerResponse>
        {
            Success = true,
            Message = "Customer updated successfully",
            Data = new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _customerService.DeleteAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool> { Success = false, Message = "Customer not found" });

        return Ok(new ApiResponse<bool> { Success = true, Message = "Customer deleted successfully" });
    }
}
