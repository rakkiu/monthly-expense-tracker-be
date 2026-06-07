namespace ExpenseTracker.Application.DTOs;

public record MonthlySummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    decimal IncomeChangePercent,
    decimal ExpenseChangePercent
);

public record CategoryBreakdownItem(
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount,
    decimal Percentage
);

public record TrendItem(
    string Date,
    decimal Amount
);

public record TopCategoryItem(
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount
);
