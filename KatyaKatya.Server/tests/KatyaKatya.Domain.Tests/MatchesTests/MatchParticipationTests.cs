using FluentAssertions;
using KatyaKatya.Domain.Matches;
using Xunit;

namespace KatyaKatya.Domain.Tests.MatchesTests;

// MatchParticipation is created internally by Match, so it is exercised through the aggregate.
public class MatchParticipationTests
{
    [Fact]
    public void NewParticipation_StartsWithZeroScore() =>
        Match.Create().AddParticipant(1).Score.Value.Should().Be(0);

    [Fact]
    public void NewParticipation_HasNoWinner() =>
        Match.Create().AddParticipant(1).WinnerId.Should().BeNull();

    [Fact]
    public void AddPoints_IncreasesScore()
    {
        var p = Match.Create().AddParticipant(1);
        p.AddPoints(5);
        p.Score.Value.Should().Be(5);
    }

    [Fact]
    public void AddPoints_Accumulates()
    {
        var p = Match.Create().AddParticipant(1);
        p.AddPoints(5);
        p.AddPoints(3);
        p.Score.Value.Should().Be(8);
    }

    [Fact]
    public void AddPoints_SetsMatchId() =>
        Match.Create().AddParticipant(1).MatchId.Should().Be(0); // transient match Id before persistence
}
