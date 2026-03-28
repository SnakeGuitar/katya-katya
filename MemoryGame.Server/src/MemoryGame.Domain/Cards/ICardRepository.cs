namespace MemoryGame.Domain.Cards;

public interface ICardRepository
{
    Task<Deck?> GetDeckByIdAsync(int id);
    Task<IReadOnlyList<Deck>> GetDecksByMatchIdAsync(int matchId);
    Task AddDeckAsync(Deck deck);
}
