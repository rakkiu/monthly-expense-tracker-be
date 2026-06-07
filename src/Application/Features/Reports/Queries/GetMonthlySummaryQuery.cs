using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Reports.Queries;

public record GetMonthlySummaryQuery(int? Month, int? Year) : IRequest<MonthlySummaryDto>;

public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlySummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = DateTime.UtcNow;
        var month = query.Month ?? now.Month;
        var year = query.Year ?? now.Year;

        var (income, expense) = await GetTotals(userId, month, year, cancellationToken);

        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;
        var (prevIncome, prevExpense) = await GetTotals(userId, prevMonth, prevYear, cancellationToken);

        return new MonthlySummaryDto(
            income, expense, income - expense,
            prevIncome > 0 ? Math.Round((income - prevIncome) / prevIncome * 100, 1) : 0,
            prevExpense > 0 ? Math.Round((expense - prevExpense) / prevExpense * 100, 1) : 0
        );
    }

    private async Task<(decimal income, decimal expense)> GetTotals(Guid userId, int month, int year, CancellationToken ct)
    {
        var income = await _context.Transactions
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == TransactionType.Income
                && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0;

        var expense = await _context.Transactions
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0;

        return (income, expense);
    }
}
