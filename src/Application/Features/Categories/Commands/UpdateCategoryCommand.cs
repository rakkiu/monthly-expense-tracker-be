using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, UpdateCategoryRequest Request) : IRequest<CategoryDto>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Color).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$");
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.UserId == userId && !c.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found");

        if (category.IsDefault)
            throw new DomainException("Cannot edit default categories");

        category.Name = command.Request.Name;
        category.Color = command.Request.Color;
        category.Icon = command.Request.Icon;

        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Type.ToString(), category.Color, category.Icon, category.IsDefault);
    }
}
