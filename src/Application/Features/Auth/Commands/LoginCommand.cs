using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Request.Email.ToLower(), cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");

        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);

        return new AuthResponse(accessToken, expiresAt,
            new UserDto(user.Id, user.FullName, user.Email));
    }
}
