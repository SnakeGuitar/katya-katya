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
    /// Snapshot of participants at game start (username → userId/isGuest),
    /// used to persist match stats even after players leave.
    /// </summary>
    public IReadOnlyDictionary<string, (int UserId, bool IsGuest)> Participants { get; }

    /// <summary>
    /// Creates a new game session with a shuffled board.
    /// </summary>
    /// <param name="cardCount">Total cards on the board (must be even).</param>
    /// <param name="turnTimeSeconds">Seconds per turn.</param>
    /// <param name="playerUsernames">Usernames of participating players in join order.</param>
    public GameSession(int cardCount, int turnTimeSeconds, IEnumerable<(string Username, int UserId, bool IsGuest)> participants)
    {
        TurnTimeSeconds = turnTimeSeconds;
        var list = participants.ToList();
        TurnOrder = list.Select(p => p.Username).ToList();
        Scores = new ConcurrentDictionary<string, int>();
        Participants = list.ToDictionary(p => p.Username, p => (p.UserId, p.IsGuest));

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
        // Real assets from the revival gallery
        var availableImages = new List<string>
        {
            "katya-1/katya-1-no-background",
            "katya-moods/main/katya-main-no-background",
            "katya-moods/in-love/katya-in-love-no-background",
            "katya-moods/shy/katya-shy-2-no-background",
            "katya-moods/sitting/katya-sit-no-background",
            "katya-moods/standing/sketch-katya-standing-no-background",
            "yumiko-1/yumiko-1-original",
            "akari-1/akari-1-original",
            "katya-moods/happy/katya-happy",
            "katya-moods/shy/katya-shy-3",
            "katya-1/katya-1-original-border",
            "katya-moods/main/sketch-katya-main-no-background",
            "katya-moods/in-love/sketch-katya-in-love-no-background",
            "katya-moods/shy/sketch-katya-shy-no-background",
            "katya-moods/happy/katya-happy", // Duplicate as fallback
            "katya-moods/shy/katya-shy-3",   // Duplicate as fallback
            "yumiko-1/yumiko-1-original",   // Duplicate as fallback
            "akari-1/akari-1-original"      // Duplicate as fallback
        };

        var pairCount = cardCount / 2;
        var cards = new List<GameCard>();

        for (var i = 0; i < pairCount; i++)
        {
            var imageId = availableImages[i % availableImages.Count];
            cards.Add(new GameCard(0, imageId));
            cards.Add(new GameCard(0, imageId));
        }

        // Shuffle
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var j = Rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        // Assign final indices
        for (var i = 0; i < cards.Count; i++)
            cards[i] = new GameCard(i, cards[i].ImageIdentifier);

        return cards;
    }
}
