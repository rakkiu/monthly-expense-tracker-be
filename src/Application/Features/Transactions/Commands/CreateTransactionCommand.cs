using AutoMapper;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Transactions.Commands;

public record CreateTransactionCommand(CreateTransactionRequest Request) : IRequest<TransactionDto>;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Type).Must(t => t == "INCOME" || t == "EXPENSE");
        RuleFor(x => x.Request.CategoryId).NotEmpty();
        RuleFor(x => x.Request.TransactionDate).NotEmpty();
    }
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public CreateTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var userId = _currentUser.UserId!.Value;

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId && !c.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found");

        var type = Enum.Parse<TransactionType>(request.Type, true);
        if (category.Type != type)
            throw new FluentValidation.ValidationException("Category type does not match transaction type");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = type,
            Description = request.Description,
            TransactionDate = DateOnly.Parse(request.TransactionDate),
            IsRecurring = request.IsRecurring
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with category
        var saved = await _context.Transactions
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == transaction.Id, cancellationToken);

        return _mapper.Map<TransactionDto>(saved);
    }
}
