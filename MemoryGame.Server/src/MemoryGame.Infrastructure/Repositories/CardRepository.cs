using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Cards;
using MemoryGame.Infrastructure.Persistence;

namespace MemoryGame.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly MemoryGameDbContext _context;

    public CardRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<Deck?> GetDeckByIdAsync(int id)
    {
        return await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IReadOnlyList<Deck>> GetDecksByMatchIdAsync(int matchId)
    {
        return await _context.Decks
            .Include(d => d.Cards)
            .Where(d => d.MatchId == matchId)
            .ToListAsync();
    }

    public async Task AddDeckAsync(Deck deck)
    {
        await _context.Decks.AddAsync(deck);
    }
}
