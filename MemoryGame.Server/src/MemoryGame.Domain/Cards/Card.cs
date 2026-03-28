using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Cards;

public class Card : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int DeckId { get; private set; }

    private Card() { }

    public static Card Create(string name, int deckId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Card name cannot be empty.");

        if (name.Length > 30)
            throw new DomainException("Card name cannot exceed 30 characters.");

        if (description is not null && description.Length > 80)
            throw new DomainException("Card description cannot exceed 80 characters.");

        return new Card
        {
            Name = name,
            DeckId = deckId,
            Description = description
        };
    }
}
