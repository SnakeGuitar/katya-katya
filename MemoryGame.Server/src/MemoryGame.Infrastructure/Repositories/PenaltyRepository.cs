using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Penalties;
using MemoryGame.Infrastructure.Persistence;

namespace MemoryGame.Infrastructure.Repositories;

public class PenaltyRepository : IPenaltyRepository
{
    private readonly MemoryGameDbContext _context;

    public PenaltyRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<Penalty?> GetByIdAsync(int id)
    {
        return await _context.Penalties.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Penalty>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.Penalties
            .Where(p => p.UserId == userId && p.IsActive())
            .ToListAsync();
    }

    public async Task AddAsync(Penalty penalty)
    {
        await _context.Penalties.AddAsync(penalty);
    }
}
