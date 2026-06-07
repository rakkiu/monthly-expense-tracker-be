using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.UserId == userId && !c.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found");

        if (category.IsDefault)
            throw new DomainException("Cannot delete default categories");

        category.IsDeleted = true;

        var uncategorized = await _context.Categories
            .FirstAsync(c => c.UserId == userId && c.IsDefault && c.Name == "Khác" && c.Type == category.Type, cancellationToken);

        await _context.Transactions
            .Where(t => t.CategoryId == command.Id)
            .ForEachAsync(t => t.CategoryId = uncategorized.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
