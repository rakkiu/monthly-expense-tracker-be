using ExpenseTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Transactions.Commands;

public record DeleteTransactionCommand(Guid Id) : IRequest<Unit>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.UserId == userId && !t.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Transaction not found");

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
