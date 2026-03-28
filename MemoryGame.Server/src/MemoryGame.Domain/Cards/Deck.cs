using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Cards;

public class Deck : BaseEntity
{
    public string Name { get; private set; } = null!;
    public int MatchId { get; private set; }

    private readonly List<Card> _cards = [];
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    private Deck() { }

    public static Deck Create(string name, int matchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Deck name cannot be empty.");

        if (name.Length > 30)
            throw new DomainException("Deck name cannot exceed 30 characters.");

        return new Deck
        {
            Name = name,
            MatchId = matchId
        };
    }

    public Card AddCard(string name, string? description = null)
    {
        var card = Card.Create(name, Id, description);
        _cards.Add(card);
        return card;
    }
}
