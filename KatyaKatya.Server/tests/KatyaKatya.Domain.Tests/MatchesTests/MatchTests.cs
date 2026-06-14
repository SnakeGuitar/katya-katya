using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Common.Enums;
using KatyaKatya.Domain.Matches;
using Xunit;

namespace KatyaKatya.Domain.Tests.MatchesTests;

public class MatchTests
{
    [Fact]
    public void Create_StatusIsInProgress() =>
        Match.Create().Status.Should().Be(MatchStatus.InProgress);

    [Fact]
    public void Create_SetsStartDateTime() =>
        Match.Create().StartDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

    [Fact]
    public void Create_EndDateTimeIsNull() =>
        Match.Create().EndDateTime.Should().BeNull();

    [Fact]
    public void Create_HasNoParticipations() =>
        Match.Create().Participations.Should().BeEmpty();

    [Fact]
    public void AddParticipant_AddsOneParticipation()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        match.Participations.Should().HaveCount(1);
    }

    [Fact]
    public void AddParticipant_ReturnsParticipationWithUserId() =>
        Match.Create().AddParticipant(99).UserId.Should().Be(99);

    [Fact]
    public void AddParticipant_SameUserTwice_Throws()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        ((Action)(() => match.AddParticipant(1))).Should().Throw<DomainException>();
    }

    [Fact]
    public void AddParticipant_AfterFinish_Throws()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        match.Finish(1);
        ((Action)(() => match.AddParticipant(2))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Finish_SetsStatusFinished()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        match.Finish(1);
        match.Status.Should().Be(MatchStatus.Finished);
    }

    [Fact]
    public void Finish_SetsEndDateTime()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        match.Finish(1);
        match.EndDateTime.Should().NotBeNull();
    }

    [Fact]
    public void Finish_AssignsWinnerToParticipations()
    {
        var match = Match.Create();
        var p = match.AddParticipant(1);
        match.Finish(1);
        p.WinnerId.Should().Be(1);
    }

    [Fact]
    public void Finish_WithWinnerNotParticipant_Throws()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        ((Action)(() => match.Finish(999))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Finish_WhenAlreadyFinished_Throws()
    {
        var match = Match.Create();
        match.AddParticipant(1);
        match.Finish(1);
        ((Action)(() => match.Finish(1))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_SetsStatusCancelled()
    {
        var match = Match.Create();
        match.Cancel();
        match.Status.Should().Be(MatchStatus.Cancelled);
    }

    [Fact]
    public void Cancel_SetsEndDateTime()
    {
        var match = Match.Create();
        match.Cancel();
        match.EndDateTime.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenNotInProgress_Throws()
    {
        var match = Match.Create();
        match.Cancel();
        ((Action)match.Cancel).Should().Throw<DomainException>();
    }
}
