using ModelProject.Entities;

namespace ModelProject.Services.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
