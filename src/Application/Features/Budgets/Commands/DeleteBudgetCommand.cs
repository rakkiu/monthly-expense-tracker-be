using ExpenseTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Budgets.Commands;

public record DeleteBudgetCommand(Guid Id) : IRequest<Unit>;

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteBudgetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteBudgetCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var budget = await _context.MonthlyBudgets
            .FirstOrDefaultAsync(b => b.Id == command.Id && b.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Budget not found");

        _context.MonthlyBudgets.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
