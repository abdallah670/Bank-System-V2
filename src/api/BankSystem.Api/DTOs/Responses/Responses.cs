namespace BankSystem.Api.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
}

public class UserResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
}

public class CustomerResponse
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AccountResponse> Accounts { get; set; } = new();
}

public class AccountResponse
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransactionResponse
{
    public long TransactionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? FromAccountId { get; set; }
    public string? FromAccountNumber { get; set; }
    public int? ToAccountId { get; set; }
    public string? ToAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TransferResponse
{
    public long FromTransactionId { get; set; }
    public long ToTransactionId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int FromAccountId { get; set; }
    public decimal FromPreviousBalance { get; set; }
    public decimal FromNewBalance { get; set; }
    public int ToAccountId { get; set; }
    public decimal ToPreviousBalance { get; set; }
    public decimal ToNewBalance { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogResponse
{
    public long AuditId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class ErrorResponse
{
    public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public string Title { get; set; } = "Error";
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}
