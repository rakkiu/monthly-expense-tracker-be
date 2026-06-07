using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Categories.Commands;

public record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<CategoryDto>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Type).Must(t => t == "INCOME" || t == "EXPENSE");
        RuleFor(x => x.Request.Color).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$");
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var request = command.Request;

        var existing = await _context.Categories
            .AnyAsync(c => c.UserId == userId && c.Name == request.Name && c.Type == Enum.Parse<TransactionType>(request.Type, true) && !c.IsDeleted, cancellationToken);

        if (existing)
            throw new DomainException("Category with this name already exists");

        var count = await _context.Categories.CountAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);
        if (count >= 50)
            throw new DomainException("Maximum 50 categories allowed");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Type = Enum.Parse<TransactionType>(request.Type, true),
            Color = request.Color,
            Icon = request.Icon
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Type.ToString(), category.Color, category.Icon, category.IsDefault);
    }
}
