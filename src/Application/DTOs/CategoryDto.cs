namespace ExpenseTracker.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Type,
    string Color,
    string? Icon,
    bool IsDefault
);

public record CreateCategoryRequest(
    string Name,
    string Type,
    string Color,
    string? Icon
);

public record UpdateCategoryRequest(
    string Name,
    string Color,
    string? Icon
);
