using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Budgets.Queries;

public record GetBudgetsQuery(int? Month, int? Year) : IRequest<List<BudgetDto>>;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, List<BudgetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetBudgetsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<List<BudgetDto>> Handle(GetBudgetsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = DateTime.UtcNow;
        var monthYear = new DateOnly(query.Year ?? now.Year, query.Month ?? now.Month, 1);

        var budgets = await _context.MonthlyBudgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.MonthYear == monthYear)
            .ToListAsync(cancellationToken);

        var categoryIds = budgets.Select(b => b.CategoryId).ToList();

        var spentAmounts = await _context.Transactions
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == Domain.Enums.TransactionType.Expense
                && t.TransactionDate.Year == monthYear.Year
                && t.TransactionDate.Month == monthYear.Month
                && categoryIds.Contains(t.CategoryId))
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var spentDict = spentAmounts.ToDictionary(x => x.CategoryId, x => x.Total);

        return budgets.Select(b =>
        {
            var spent = spentDict.GetValueOrDefault(b.CategoryId, 0);
            var remaining = b.Amount - spent;
            var percent = b.Amount > 0 ? Math.Round(spent / b.Amount * 100, 1) : 0;
            return new BudgetDto(
                b.Id, b.CategoryId, b.Category.Name, b.Category.Color,
                b.Amount, spent, remaining, percent);
        }).ToList();
    }
}
