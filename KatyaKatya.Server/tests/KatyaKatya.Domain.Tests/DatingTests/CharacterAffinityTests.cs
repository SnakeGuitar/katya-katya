using FluentAssertions;
using KatyaKatya.Domain.Dating;
using Xunit;

namespace KatyaKatya.Domain.Tests.DatingTests;

public class CharacterAffinityTests
{
    [Fact]
    public void Create_StartsAtStrangerNeutral()
    {
        var affinity = CharacterAffinity.Create(1, 2);

        affinity.UserId.Should().Be(1);
        affinity.CharacterId.Should().Be(2);
        affinity.LovePoints.Should().Be(0);
        affinity.Level.Should().Be(AffinityLevel.Stranger);
        affinity.Mood.Should().Be(RelationshipMood.Neutral);
    }

    [Fact]
    public void ApplyGift_AddsLovePointsAndGiftCount()
    {
        var affinity = CharacterAffinity.Create(1, 2);

        affinity.ApplyGift(12, RelationshipMood.Happy);

        affinity.LovePoints.Should().Be(12);
        affinity.TotalGiftsReceived.Should().Be(1);
        affinity.Mood.Should().Be(RelationshipMood.Happy);
        affinity.LastInteractionAt.Should().NotBeNull();
    }

    [Fact]
    public void CompleteDate_AddsLovePointsAndDateCount()
    {
        var affinity = CharacterAffinity.Create(1, 2);

        affinity.CompleteDate(25, RelationshipMood.Shy);

        affinity.LovePoints.Should().Be(25);
        affinity.TotalDates.Should().Be(1);
        affinity.Mood.Should().Be(RelationshipMood.Shy);
    }

    [Fact]
    public void ApplyGift_NegativeDelta_DoesNotGoBelowZero()
    {
        var affinity = CharacterAffinity.Create(1, 2);

        affinity.ApplyGift(-5, RelationshipMood.Upset);

        affinity.LovePoints.Should().Be(0);
    }
}
