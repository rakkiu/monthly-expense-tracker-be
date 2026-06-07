namespace ExpenseTracker.Application.DTOs;

public record TransactionDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal Amount,
    string Type,
    string? Description,
    string TransactionDate,
    bool IsRecurring,
    DateTime CreatedAt
);

public record CreateTransactionRequest(
    decimal Amount,
    string Type,
    Guid CategoryId,
    string? Description,
    string TransactionDate,
    bool IsRecurring
);

public record UpdateTransactionRequest(
    decimal Amount,
    string Type,
    Guid CategoryId,
    string? Description,
    string TransactionDate,
    bool IsRecurring
);

public record TransactionListRequest(
    int? Month,
    int? Year,
    Guid? CategoryId,
    string? Type,
    string? Keyword,
    string? SortBy,
    string? SortDir,
    int Page = 1,
    int PageSize = 20
);
