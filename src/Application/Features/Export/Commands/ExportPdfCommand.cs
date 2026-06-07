using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Export.Commands;

public record ExportPdfCommand(int? Month, int? Year) : IRequest<byte[]>;

public class ExportPdfCommandHandler : IRequestHandler<ExportPdfCommand, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ExportPdfCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<byte[]> Handle(ExportPdfCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = DateTime.UtcNow;
        var month = command.Month ?? now.Month;
        var year = command.Year ?? now.Year;

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted
                && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        // Simple text-based PDF placeholder - will use QuestPDF in production
        var lines = new List<string>
        {
            $"Monthly Expense Report - {month:D2}/{year}",
            $"User: {user?.FullName ?? "N/A"}",
            $"Total Income: {income:N0} VND",
            $"Total Expense: {expense:N0} VND",
            $"Balance: {income - expense:N0} VND",
            "",
            "Transactions:",
            "Date,Ttype,Category,Description,Amount"
        };

        foreach (var t in transactions)
        {
            lines.Add($"{t.TransactionDate},{t.Type},{t.Category.Name},{t.Description ?? ""},{t.Amount}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
    }
}
