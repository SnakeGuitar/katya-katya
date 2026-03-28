using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Users;
using MemoryGame.Infrastructure.Persistence;

namespace MemoryGame.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly MemoryGameDbContext _context;

    public UserSessionRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession?> GetByTokenAsync(string token)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Token == token);
    }

    public async Task<IReadOnlyList<UserSession>> GetByUserIdAsync(int userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(UserSession session)
    {
        await _context.UserSessions.AddAsync(session);
    }

    public void Remove(UserSession session)
    {
        _context.UserSessions.Remove(session);
    }
}
