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
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAuditService _auditService;

    public TransactionsController(ITransactionService transactionService, IAuditService auditService)
    {
        _transactionService = transactionService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        var (items, totalCount) = await _transactionService.GetAllAsync(
            request.Page, request.PageSize, request.StartDate, request.EndDate, request.Type);

        var transactions = items.Select(t => new TransactionResponse
        {
            TransactionId = t.TransactionId,
            Type = t.Type.ToString(),
            FromAccountId = t.FromAccountId,
            FromAccountNumber = t.FromAccount?.AccountNumber,
            ToAccountId = t.ToAccountId,
            ToAccountNumber = t.ToAccount?.AccountNumber,
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            ReferenceNumber = t.ReferenceNumber,
            Description = t.Description,
            Status = t.Status.ToString(),
            CreatedByName = t.User?.Username ?? "",
            CreatedAt = t.CreatedAt
        });

        return Ok(new PagedResponse<TransactionResponse>
        {
            Items = transactions,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(long id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction == null)
            return NotFound(new ApiResponse<TransactionResponse> { Success = false, Message = "Transaction not found" });

        return Ok(new ApiResponse<TransactionResponse>
        {
            Success = true,
            Data = new TransactionResponse
            {
                TransactionId = transaction.TransactionId,
                Type = transaction.Type.ToString(),
                FromAccountId = transaction.FromAccountId,
                FromAccountNumber = transaction.FromAccount?.AccountNumber,
                ToAccountId = transaction.ToAccountId,
                ToAccountNumber = transaction.ToAccount?.AccountNumber,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                ReferenceNumber = transaction.ReferenceNumber,
                Description = transaction.Description,
                Status = transaction.Status.ToString(),
                CreatedByName = transaction.User?.Username ?? "",
                CreatedAt = transaction.CreatedAt
            }
        });
    }

    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<TransactionResponse>>>> GetRecent([FromQuery] int count = 10)
    {
        var transactions = await _transactionService.GetRecentAsync(count);
        var response = transactions.Select(t => new TransactionResponse
        {
            TransactionId = t.TransactionId,
            Type = t.Type.ToString(),
            FromAccountId = t.FromAccountId,
            FromAccountNumber = t.FromAccount?.AccountNumber,
            ToAccountId = t.ToAccountId,
            ToAccountNumber = t.ToAccount?.AccountNumber,
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            ReferenceNumber = t.ReferenceNumber,
            Description = t.Description,
            Status = t.Status.ToString(),
            CreatedByName = t.User?.Username ?? "",
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(new ApiResponse<List<TransactionResponse>>
        {
            Success = true,
            Data = response
        });
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Deposit([FromBody] DepositRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var transaction = await _transactionService.DepositAsync(request.AccountId, request.Amount, request.Description, userId);

            return Ok(new ApiResponse<TransactionResponse>
            {
                Success = true,
                Message = "Deposit successful",
                Data = new TransactionResponse
                {
                    TransactionId = transaction.TransactionId,
                    Type = transaction.Type.ToString(),
                    ToAccountId = transaction.ToAccountId,
                    ToAccountNumber = transaction.ToAccount?.AccountNumber,
                    Amount = transaction.Amount,
                    BalanceAfter = transaction.BalanceAfter,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Description = transaction.Description,
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<TransactionResponse> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Withdraw([FromBody] WithdrawRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var transaction = await _transactionService.WithdrawAsync(request.AccountId, request.Amount, request.Description, userId);

            return Ok(new ApiResponse<TransactionResponse>
            {
                Success = true,
                Message = "Withdrawal successful",
                Data = new TransactionResponse
                {
                    TransactionId = transaction.TransactionId,
                    Type = transaction.Type.ToString(),
                    FromAccountId = transaction.FromAccountId,
                    FromAccountNumber = transaction.FromAccount?.AccountNumber,
                    Amount = transaction.Amount,
                    BalanceAfter = transaction.BalanceAfter,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Description = transaction.Description,
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<TransactionResponse> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<TransferResponse>>> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (fromTransaction, toTransaction) = await _transactionService.TransferAsync(
                request.FromAccountId, request.ToAccountId, request.Amount, request.Description, userId);

            return Ok(new ApiResponse<TransferResponse>
            {
                Success = true,
                Message = "Transfer successful",
                Data = new TransferResponse
                {
                    FromTransactionId = fromTransaction!.TransactionId,
                    ToTransactionId = toTransaction!.TransactionId,
                    ReferenceNumber = fromTransaction.ReferenceNumber,
                    FromAccountId = fromTransaction.FromAccountId!.Value,
                    FromPreviousBalance = fromTransaction.BalanceAfter + request.Amount,
                    FromNewBalance = fromTransaction.BalanceAfter,
                    ToAccountId = toTransaction.ToAccountId!.Value,
                    ToPreviousBalance = toTransaction.BalanceAfter - request.Amount,
                    ToNewBalance = toTransaction.BalanceAfter,
                    Timestamp = fromTransaction.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<TransferResponse> { Success = false, Message = ex.Message });
        }
    }
}
