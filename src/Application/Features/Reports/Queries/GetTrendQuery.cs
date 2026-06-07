using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Reports.Queries;

public record GetTrendQuery(int Months = 6) : IRequest<List<TrendItem>>;

public class GetTrendQueryHandler : IRequestHandler<GetTrendQuery, List<TrendItem>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTrendQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TrendItem>> Handle(GetTrendQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var months = Math.Clamp(query.Months, 1, 6);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months + 1));

        var data = await _context.Transactions
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate >= startDate)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(t => t.Amount) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return data.Select(d => new TrendItem($"{d.Year}-{d.Month:D2}", d.Amount)).ToList();
    }
}
