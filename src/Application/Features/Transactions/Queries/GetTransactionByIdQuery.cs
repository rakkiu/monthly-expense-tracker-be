using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Transactions.Queries;

public record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDto>;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Transaction not found");

        return transaction;
    }
}
