using System.ComponentModel.DataAnnotations;

namespace BankSystem.Api.DTOs.Requests;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = "BankAdmin";
}

public class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class CreateCustomerRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "United States";
    public DateTime? DateOfBirth { get; set; }
}

public class UpdateCustomerRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "United States";
}

public class CreateAccountRequest
{
    [Required]
    public int CustomerId { get; set; }
    [Required]
    public string AccountType { get; set; } = "Savings";
    public string Currency { get; set; } = "USD";
    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; } = 0;
}

public class DepositRequest
{
    [Required]
    public int AccountId { get; set; }
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class WithdrawRequest
{
    [Required]
    public int AccountId { get; set; }
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class TransferRequest
{
    [Required]
    public int FromAccountId { get; set; }
    [Required]
    public int ToAccountId { get; set; }
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
}
