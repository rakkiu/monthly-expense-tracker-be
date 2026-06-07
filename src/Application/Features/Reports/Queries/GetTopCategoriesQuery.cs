using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Reports.Queries;

public record GetTopCategoriesQuery(int? Month, int? Year) : IRequest<List<TopCategoryItem>>;

public class GetTopCategoriesQueryHandler : IRequestHandler<GetTopCategoriesQuery, List<TopCategoryItem>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTopCategoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TopCategoryItem>> Handle(GetTopCategoriesQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = DateTime.UtcNow;
        var month = query.Month ?? now.Month;
        var year = query.Year ?? now.Year;

        return await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new TopCategoryItem(g.Key.CategoryId, g.Key.Name, g.Key.Color, g.Sum(t => t.Amount)))
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .ToListAsync(cancellationToken);
    }
}
