using System.Collections.Concurrent;

namespace MemoryGame.Application.Lobbies.Models;

/// <summary>
/// Manages the state of an active memory game: the board, turns, scores, and match detection.
/// </summary>
public class GameSession
{
    private static readonly Random Rng = new();

    public List<GameCard> Board { get; }
    public List<string> TurnOrder { get; }
    public int CurrentTurnIndex { get; private set; }
    public ConcurrentDictionary<string, int> Scores { get; }
    public int TurnTimeSeconds { get; }
    public GameCard? FirstFlippedCard { get; private set; }
    public bool IsWaitingForSecondFlip { get; private set; }
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Creates a new game session with a shuffled board.
    /// </summary>
    /// <param name="cardCount">Total cards on the board (must be even).</param>
    /// <param name="turnTimeSeconds">Seconds per turn.</param>
    /// <param name="playerUsernames">Usernames of participating players in join order.</param>
    public GameSession(int cardCount, int turnTimeSeconds, IEnumerable<string> playerUsernames)
    {
        TurnTimeSeconds = turnTimeSeconds;
        TurnOrder = new List<string>(playerUsernames);
        Scores = new ConcurrentDictionary<string, int>();

        foreach (var username in TurnOrder)
            Scores[username] = 0;

        Board = GenerateBoard(cardCount);
    }

    /// <summary>
    /// Returns the username of the player whose turn it currently is.
    /// </summary>
    public string CurrentPlayer => TurnOrder[CurrentTurnIndex];

    /// <summary>
    /// Attempts to flip a card. Returns the flipped card, or null if the flip is invalid.
    /// </summary>
    public GameCard? FlipCard(int cardIndex, string playerUsername)
    {
        if (IsFinished || playerUsername != CurrentPlayer)
            return null;

        if (cardIndex < 0 || cardIndex >= Board.Count)
            return null;

        var card = Board[cardIndex];

        if (card.IsFaceUp || card.IsMatched)
            return null;

        card.IsFaceUp = true;

        if (!IsWaitingForSecondFlip)
        {
            FirstFlippedCard = card;
            IsWaitingForSecondFlip = true;
            return card;
        }

        IsWaitingForSecondFlip = false;
        return card;
    }

    /// <summary>
    /// Evaluates whether the two flipped cards are a match. Updates scores and board state.
    /// Returns true if the cards match.
    /// </summary>
    public bool EvaluateMatch(GameCard first, GameCard second)
    {
        if (first.ImageIdentifier == second.ImageIdentifier)
        {
            first.IsMatched = true;
            second.IsMatched = true;
            Scores[CurrentPlayer]++;

            if (Board.All(c => c.IsMatched))
                IsFinished = true;

            return true;
        }

        first.IsFaceUp = false;
        second.IsFaceUp = false;
        AdvanceTurn();
        return false;
    }

    /// <summary>
    /// Advances the turn to the next player.
    /// </summary>
    public void AdvanceTurn()
    {
        FirstFlippedCard = null;
        IsWaitingForSecondFlip = false;
        CurrentTurnIndex = (CurrentTurnIndex + 1) % TurnOrder.Count;
    }

    /// <summary>
    /// Removes a player from the turn order. If only one remains, the game ends.
    /// </summary>
    public void RemovePlayer(string username)
    {
        var wasCurrentPlayer = CurrentPlayer == username;
        TurnOrder.Remove(username);
        Scores.TryRemove(username, out _);

        if (TurnOrder.Count <= 1)
        {
            IsFinished = true;
            return;
        }

        if (wasCurrentPlayer && CurrentTurnIndex >= TurnOrder.Count)
            CurrentTurnIndex = 0;
    }

    /// <summary>
    /// Determines the winner based on the highest score.
    /// </summary>
    public string? GetWinner()
    {
        if (Scores.IsEmpty) return null;
        var max = Scores.Max(s => s.Value);
        var winners = Scores.Where(s => s.Value == max).Select(s => s.Key).ToList();
        return winners.Count == 1 ? winners[0] : null;
    }

    private static List<GameCard> GenerateBoard(int cardCount)
    {
        var pairCount = cardCount / 2;
        var cards = new List<GameCard>();

        for (var i = 0; i < pairCount; i++)
        {
            var imageId = $"card_{i:D3}";
            cards.Add(new GameCard(0, imageId));
            cards.Add(new GameCard(0, imageId));
        }

        for (var i = cards.Count - 1; i > 0; i--)
        {
            var j = Rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        for (var i = 0; i < cards.Count; i++)
            cards[i] = new GameCard(i, cards[i].ImageIdentifier);

        return cards;
    }
}
