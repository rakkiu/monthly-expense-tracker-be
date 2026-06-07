using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Budgets.Commands;

public record SetBudgetCommand(Guid CategoryId, decimal Amount, string MonthYear) : IRequest<Unit>;

public class SetBudgetCommandValidator : AbstractValidator<SetBudgetCommand>
{
    public SetBudgetCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.MonthYear).NotEmpty().Matches(@"^\d{4}-\d{2}$");
    }
}

public class SetBudgetCommandHandler : IRequestHandler<SetBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetBudgetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(SetBudgetCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var monthYear = DateOnly.Parse($"{command.MonthYear}-01");

        var existing = await _context.MonthlyBudgets
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == command.CategoryId && b.MonthYear == monthYear, cancellationToken);

        if (existing is not null)
        {
            existing.Amount = command.Amount;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.MonthlyBudgets.Add(new MonthlyBudget
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = command.CategoryId,
                Amount = command.Amount,
                MonthYear = monthYear
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
