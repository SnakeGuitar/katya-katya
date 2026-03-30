namespace MemoryGame.Application.Lobbies.Models;

/// <summary>
/// Represents a single card on the game board during an active match.
/// </summary>
public class GameCard
{
    public int Index { get; }
    public string ImageIdentifier { get; }
    public bool IsFaceUp { get; set; }
    public bool IsMatched { get; set; }

    public GameCard(int index, string imageIdentifier)
    {
        Index = index;
        ImageIdentifier = imageIdentifier;
    }
}
