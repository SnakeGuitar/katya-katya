namespace MemoryGame.Domain.Users;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByTokenAsync(string token);
    Task<IReadOnlyList<UserSession>> GetByUserIdAsync(int userId);
    Task AddAsync(UserSession session);
    void Remove(UserSession session);
}
