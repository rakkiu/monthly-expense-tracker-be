using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface ITokenService
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
