using FluentAssertions;
using KatyaKatya.Application.Lobbies.Models;
using Xunit;

namespace KatyaKatya.Application.Tests.Lobbies;

public class GameSessionTests
{
    private static GameSession Session(int cardCount = 4) =>
        new(cardCount, 30, [("alice", 1, false), ("bob", 2, true)]);

    [Fact]
    public void Constructor_BuildsTurnOrderFromParticipants() =>
        Session().TurnOrder.Should().Equal("alice", "bob");

    [Fact]
    public void Constructor_InitializesEachScoreToZero() =>
        Session().Scores["alice"].Should().Be(0);

    [Fact]
    public void Constructor_StartsAtTurnIndexZero() =>
        Session().CurrentTurnIndex.Should().Be(0);

    [Fact]
    public void Constructor_CurrentPlayerIsFirstParticipant() =>
        Session().CurrentPlayer.Should().Be("alice");

    [Fact]
    public void Constructor_BoardHasRequestedCardCount() =>
        Session(8).Board.Should().HaveCount(8);

    [Fact]
    public void Constructor_SnapshotsParticipantUserId() =>
        Session().Participants["bob"].UserId.Should().Be(2);

    [Fact]
    public void Constructor_GameNotFinished() =>
        Session().IsFinished.Should().BeFalse();

    [Fact]
    public void FlipCard_ByPlayerNotOnTurn_ReturnsNull() =>
        Session().FlipCard(0, "bob").Should().BeNull();

    [Fact]
    public void FlipCard_ValidFlip_ReturnsCard() =>
        Session().FlipCard(0, "alice").Should().NotBeNull();

    [Fact]
    public void FlipCard_ValidFlip_TurnsCardFaceUp() =>
        Session().FlipCard(0, "alice")!.IsFaceUp.Should().BeTrue();

    [Fact]
    public void FlipCard_FirstFlip_SetsWaitingForSecondFlip()
    {
        var s = Session();
        s.FlipCard(0, "alice");
        s.IsWaitingForSecondFlip.Should().BeTrue();
    }

    [Fact]
    public void FlipCard_IndexOutOfRange_ReturnsNull() =>
        Session().FlipCard(999, "alice").Should().BeNull();

    [Fact]
    public void FlipCard_AlreadyFaceUpCard_ReturnsNull()
    {
        var s = Session();
        s.FlipCard(0, "alice");
        s.FlipCard(0, "alice").Should().BeNull();
    }

    [Fact]
    public void EvaluateMatch_MatchingCards_ReturnsTrue() =>
        Session().EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "x")).Should().BeTrue();

    [Fact]
    public void EvaluateMatch_MatchingCards_IncrementsCurrentPlayerScore()
    {
        var s = Session();
        s.EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "x"));
        s.Scores["alice"].Should().Be(1);
    }

    [Fact]
    public void EvaluateMatch_MatchingCards_MarksBothMatched()
    {
        var first = new GameCard(0, "x");
        Session().EvaluateMatch(first, new GameCard(1, "x"));
        first.IsMatched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateMatch_MatchingCards_KeepsSameCurrentPlayer()
    {
        var s = Session();
        s.EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "x"));
        s.CurrentPlayer.Should().Be("alice");
    }

    [Fact]
    public void EvaluateMatch_NonMatchingCards_ReturnsFalse() =>
        Session().EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "y")).Should().BeFalse();

    [Fact]
    public void EvaluateMatch_NonMatchingCards_AdvancesTurn()
    {
        var s = Session();
        s.EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "y"));
        s.CurrentPlayer.Should().Be("bob");
    }

    [Fact]
    public void EvaluateMatch_NonMatchingCards_FlipsCardsFaceDown()
    {
        var first = new GameCard(0, "x") { IsFaceUp = true };
        Session().EvaluateMatch(first, new GameCard(1, "y") { IsFaceUp = true });
        first.IsFaceUp.Should().BeFalse();
    }

    [Fact]
    public void AdvanceTurn_MovesToNextPlayer()
    {
        var s = Session();
        s.AdvanceTurn();
        s.CurrentPlayer.Should().Be("bob");
    }

    [Fact]
    public void AdvanceTurn_WrapsAroundToFirstPlayer()
    {
        var s = Session();
        s.AdvanceTurn();
        s.AdvanceTurn();
        s.CurrentPlayer.Should().Be("alice");
    }

    [Fact]
    public void RemovePlayer_RemovesFromTurnOrder()
    {
        var s = new GameSession(4, 30, [("alice", 1, false), ("bob", 2, false), ("carol", 3, false)]);
        s.RemovePlayer("bob");
        s.TurnOrder.Should().NotContain("bob");
    }

    [Fact]
    public void RemovePlayer_LeavingOnePlayer_FinishesGame()
    {
        var s = Session();
        s.RemovePlayer("bob");
        s.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void GetWinner_ReturnsPlayerWithHighestScore()
    {
        var s = Session();
        s.EvaluateMatch(new GameCard(0, "x"), new GameCard(1, "x")); // alice +1
        s.GetWinner().Should().Be("alice");
    }

    [Fact]
    public void GetWinner_OnTie_ReturnsNull() =>
        Session().GetWinner().Should().BeNull();
}
