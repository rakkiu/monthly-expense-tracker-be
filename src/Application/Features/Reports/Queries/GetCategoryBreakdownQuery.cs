using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Reports.Queries;

public record GetCategoryBreakdownQuery(int? Month, int? Year) : IRequest<List<CategoryBreakdownItem>>;

public class GetCategoryBreakdownQueryHandler : IRequestHandler<GetCategoryBreakdownQuery, List<CategoryBreakdownItem>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryBreakdownQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryBreakdownItem>> Handle(GetCategoryBreakdownQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = DateTime.UtcNow;
        var month = query.Month ?? now.Month;
        var year = query.Year ?? now.Year;

        var data = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, g.Key.Color, Amount = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var total = data.Sum(d => d.Amount);

        return data.Select(d => new CategoryBreakdownItem(
            d.CategoryId, d.Name, d.Color, d.Amount,
            total > 0 ? Math.Round(d.Amount / total * 100, 1) : 0
        )).OrderByDescending(d => d.Amount).ToList();
    }
}
