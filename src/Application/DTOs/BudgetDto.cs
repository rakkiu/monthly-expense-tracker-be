namespace ExpenseTracker.Application.DTOs;

public record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal BudgetedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal PercentageUsed
);

public record SetBudgetRequest(
    Guid CategoryId,
    decimal Amount,
    string MonthYear
);
