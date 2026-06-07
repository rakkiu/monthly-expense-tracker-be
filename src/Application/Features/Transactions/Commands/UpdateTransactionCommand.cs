using AutoMapper;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Transactions.Commands;

public record UpdateTransactionCommand(Guid Id, UpdateTransactionRequest Request) : IRequest<TransactionDto>;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Type).Must(t => t == "INCOME" || t == "EXPENSE");
        RuleFor(x => x.Request.CategoryId).NotEmpty();
    }
}

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.UserId == userId && !t.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Transaction not found");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == command.Request.CategoryId && c.UserId == userId && !c.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found");

        var type = Enum.Parse<TransactionType>(command.Request.Type, true);
        if (category.Type != type)
            throw new ValidationException("Category type does not match transaction type");

        transaction.Amount = command.Request.Amount;
        transaction.Type = type;
        transaction.CategoryId = command.Request.CategoryId;
        transaction.Description = command.Request.Description;
        transaction.TransactionDate = DateOnly.Parse(command.Request.TransactionDate);
        transaction.IsRecurring = command.Request.IsRecurring;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TransactionDto>(transaction);
    }
}
