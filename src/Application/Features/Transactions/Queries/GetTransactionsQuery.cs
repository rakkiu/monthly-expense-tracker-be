using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Transactions.Queries;

public record GetTransactionsQuery(
    int? Month,
    int? Year,
    Guid? CategoryId,
    string? Type,
    string? Keyword,
    string? SortBy,
    string? SortDir,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<TransactionDto>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var q = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (query.Month.HasValue && query.Year.HasValue)
        {
            var date = new DateOnly(query.Year.Value, query.Month.Value, 1);
            q = q.Where(t => t.TransactionDate.Year == date.Year && t.TransactionDate.Month == date.Month);
        }

        if (query.CategoryId.HasValue)
            q = q.Where(t => t.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var type = Enum.Parse<Domain.Enums.TransactionType>(query.Type, true);
            q = q.Where(t => t.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(t => t.Description != null && t.Description.Contains(query.Keyword));

        q = (query.SortBy?.ToLower(), query.SortDir?.ToLower()) switch
        {
            ("amount", "asc") => q.OrderBy(t => t.Amount),
            ("amount", "desc") => q.OrderByDescending(t => t.Amount),
            _ => q.OrderByDescending(t => t.TransactionDate)
        };

        var totalItems = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedList<TransactionDto>(items, query.Page, query.PageSize, totalItems);
    }
}
