using FluentAssertions;
using MemoryGame.Application.Lobbies.Models;
using Xunit;

namespace MemoryGame.Application.Tests.Lobbies;

public class GameSessionTests
{
    // -----------------------------------------------------------------------
    // Board generation
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_GeneratesBoard_WithCorrectCardCount()
    {
        var session = new GameSession(8, 30, ["alice", "bob"]);

        session.Board.Should().HaveCount(8);
    }

    [Fact]
    public void Constructor_GeneratesBoard_WithExactPairs()
    {
        var session = new GameSession(8, 30, ["alice", "bob"]);

        var groups = session.Board.GroupBy(c => c.ImageIdentifier);
        groups.Should().AllSatisfy(g => g.Should().HaveCount(2));
    }

    [Fact]
    public void Constructor_AssignsSequentialIndices()
    {
        var session = new GameSession(6, 30, ["alice", "bob"]);

        session.Board.Select(c => c.Index)
            .Should().BeEquivalentTo(Enumerable.Range(0, 6));
    }

    [Fact]
    public void Constructor_InitializesScores_ForAllPlayers()
    {
        var session = new GameSession(4, 30, ["alice", "bob", "charlie"]);

        session.Scores.Keys.Should().BeEquivalentTo(["alice", "bob", "charlie"]);
        session.Scores.Values.Should().AllBeEquivalentTo(0);
    }

    // -----------------------------------------------------------------------
    // FlipCard — valid paths
    // -----------------------------------------------------------------------

    [Fact]
    public void FlipCard_FirstFlip_ReturnsCard_AndSetsWaitingFlag()
    {
        var session = new GameSession(4, 30, ["alice"]);

        var result = session.FlipCard(0, "alice");

        result.Should().NotBeNull();
        result!.Index.Should().Be(0);
        session.IsWaitingForSecondFlip.Should().BeTrue();
        session.FirstFlippedCard.Should().Be(result);
    }

    [Fact]
    public void FlipCard_SecondFlip_ReturnsCard_AndClearsWaitingFlag()
    {
        var session = new GameSession(4, 30, ["alice"]);

        session.FlipCard(0, "alice");
        var second = session.FlipCard(1, "alice");

        second.Should().NotBeNull();
        session.IsWaitingForSecondFlip.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // FlipCard — invalid paths
    // -----------------------------------------------------------------------

    [Fact]
    public void FlipCard_WrongPlayer_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);

        var result = session.FlipCard(0, "bob");

        result.Should().BeNull();
    }

    [Fact]
    public void FlipCard_AlreadyFaceUp_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice"]);
        session.FlipCard(0, "alice");

        var result = session.FlipCard(0, "alice");

        result.Should().BeNull();
    }

    [Fact]
    public void FlipCard_AlreadyMatched_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice"]);
        FlipAndMatchPair(session, "alice");

        var matchedIndex = session.Board.First(c => c.IsMatched).Index;
        var result = session.FlipCard(matchedIndex, "alice");

        result.Should().BeNull();
    }

    [Fact]
    public void FlipCard_IndexOutOfRange_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice"]);

        var result = session.FlipCard(99, "alice");

        result.Should().BeNull();
    }

    [Fact]
    public void FlipCard_WhenFinished_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice"]);
        MatchAllCards(session, "alice");

        var result = session.FlipCard(0, "alice");

        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // EvaluateMatch
    // -----------------------------------------------------------------------

    [Fact]
    public void EvaluateMatch_MatchingCards_ReturnsTrue_IncreasesScore()
    {
        var session = new GameSession(4, 30, ["alice"]);
        var (first, second) = GetMatchingPair(session);

        session.FlipCard(first.Index, "alice");
        session.FlipCard(second.Index, "alice");
        var isMatch = session.EvaluateMatch(first, second);

        isMatch.Should().BeTrue();
        session.Scores["alice"].Should().Be(1);
        first.IsMatched.Should().BeTrue();
        second.IsMatched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateMatch_AllCardsMatched_SetsIsFinished()
    {
        var session = new GameSession(4, 30, ["alice"]);
        MatchAllCards(session, "alice");

        session.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void EvaluateMatch_NonMatchingCards_ReturnsFalse_HidesCards_AdvancesTurn()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);
        var (first, second) = GetNonMatchingPair(session);

        session.FlipCard(first.Index, "alice");
        session.FlipCard(second.Index, "alice");
        var isMatch = session.EvaluateMatch(first, second);

        isMatch.Should().BeFalse();
        first.IsFaceUp.Should().BeFalse();
        second.IsFaceUp.Should().BeFalse();
        session.CurrentPlayer.Should().Be("bob");
    }

    // -----------------------------------------------------------------------
    // AdvanceTurn
    // -----------------------------------------------------------------------

    [Fact]
    public void AdvanceTurn_MovesToNextPlayer()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);

        session.AdvanceTurn();

        session.CurrentPlayer.Should().Be("bob");
    }

    [Fact]
    public void AdvanceTurn_WrapsAround_WhenLastPlayer()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);

        session.AdvanceTurn();
        session.AdvanceTurn();

        session.CurrentPlayer.Should().Be("alice");
    }

    // -----------------------------------------------------------------------
    // RemovePlayer
    // -----------------------------------------------------------------------

    [Fact]
    public void RemovePlayer_NonCurrentPlayer_DoesNotChangeTurn()
    {
        var session = new GameSession(4, 30, ["alice", "bob", "charlie"]);

        session.RemovePlayer("charlie");

        session.CurrentPlayer.Should().Be("alice");
    }

    [Fact]
    public void RemovePlayer_CurrentPlayerNotAtLastIndex_NextPlayerTakesTurn()
    {
        var session = new GameSession(4, 30, ["alice", "bob", "charlie"]);

        session.RemovePlayer("alice");

        session.CurrentPlayer.Should().Be("bob");
    }

    [Fact]
    public void RemovePlayer_CurrentPlayerAtLastIndex_WrapsToFirst()
    {
        var session = new GameSession(4, 30, ["alice", "bob", "charlie"]);
        session.AdvanceTurn();
        session.AdvanceTurn();

        session.RemovePlayer("charlie");

        session.CurrentPlayer.Should().Be("alice");
    }

    [Fact]
    public void RemovePlayer_OnlyOnePlayerRemaining_SetsIsFinished()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);

        session.RemovePlayer("alice");

        session.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void RemovePlayer_RemovesPlayerScore()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);

        session.RemovePlayer("alice");

        session.Scores.Should().NotContainKey("alice");
    }

    // -----------------------------------------------------------------------
    // GetWinner
    // -----------------------------------------------------------------------

    [Fact]
    public void GetWinner_SingleWinner_ReturnsUsername()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);
        FlipAndMatchPair(session, "alice");

        var winner = session.GetWinner();

        winner.Should().Be("alice");
    }

    [Fact]
    public void GetWinner_Tie_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice", "bob"]);
        FlipAndMatchPair(session, "alice");
        session.AdvanceTurn();
        FlipAndMatchPair(session, "bob");

        var winner = session.GetWinner();

        winner.Should().BeNull();
    }

    [Fact]
    public void GetWinner_EmptyScores_ReturnsNull()
    {
        var session = new GameSession(4, 30, ["alice"]);
        session.RemovePlayer("alice");

        var winner = session.GetWinner();

        winner.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static (GameCard first, GameCard second) GetMatchingPair(GameSession session)
    {
        var imageId = session.Board[0].ImageIdentifier;
        var first  = session.Board.First(c => c.ImageIdentifier == imageId);
        var second = session.Board.Last(c => c.ImageIdentifier == imageId);
        return (first, second);
    }

    private static (GameCard first, GameCard second) GetNonMatchingPair(GameSession session)
    {
        var first  = session.Board[0];
        var second = session.Board.First(c => c.ImageIdentifier != first.ImageIdentifier);
        return (first, second);
    }

    private static void FlipAndMatchPair(GameSession session, string player)
    {
        var (first, second) = GetMatchingPair(session);
        var f = session.FlipCard(first.Index, player)!;
        var s = session.FlipCard(second.Index, player)!;
        session.EvaluateMatch(f, s);
    }

    private static void MatchAllCards(GameSession session, string player)
    {
        while (!session.IsFinished)
        {
            var unmatched = session.Board.Where(c => !c.IsMatched && !c.IsFaceUp).ToList();
            if (unmatched.Count < 2) break;

            var first  = unmatched[0];
            var second = unmatched.FirstOrDefault(c => c.ImageIdentifier == first.ImageIdentifier && c.Index != first.Index);

            if (second is null)
            {
                var f = session.FlipCard(first.Index, player)!;
                var nonMatch = unmatched.First(c => c.ImageIdentifier != first.ImageIdentifier);
                var s = session.FlipCard(nonMatch.Index, player)!;
                session.EvaluateMatch(f, s);
            }
            else
            {
                var f = session.FlipCard(first.Index, player)!;
                var s = session.FlipCard(second.Index, player)!;
                session.EvaluateMatch(f, s);
            }
        }
    }
}
