using ExpenseTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == command.RefreshToken, cancellationToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
