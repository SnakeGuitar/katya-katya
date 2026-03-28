using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int? GetUserIdFromToken(string token);
    bool IsRefreshTokenEmpty(string token, int userId);
}
