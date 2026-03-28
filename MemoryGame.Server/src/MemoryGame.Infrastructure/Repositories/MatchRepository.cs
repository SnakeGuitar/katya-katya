using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Matches;
using MemoryGame.Infrastructure.Persistence;

namespace MemoryGame.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly MemoryGameDbContext _context;

    public MatchRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<Match?> GetByIdAsync(int id)
    {
        return await _context.Matches
            .Include(m => m.Participations)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IReadOnlyList<MatchParticipation>> GetHistoryByUserIdAsync(int userId)
    {
        return await _context.MatchParticipations
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.MatchId)
            .ToListAsync();
    }

    public async Task AddAsync(Match match)
    {
        await _context.Matches.AddAsync(match);
    }

    public void Update(Match match)
    {
        _context.Matches.Update(match);
    }
}
