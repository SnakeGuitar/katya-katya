using FluentAssertions;
using KatyaKatya.Domain.Common.Enums;
using KatyaKatya.Domain.Penalties;
using Xunit;

namespace KatyaKatya.Domain.Tests.PenaltyTests;

public class PenaltyTests
{
    private static readonly DateTime Future = DateTime.UtcNow.AddHours(1);
    private static readonly DateTime Past = DateTime.UtcNow.AddHours(-1);

    [Fact]
    public void Create_SetsType() =>
        Penalty.Create(PenaltyType.Warning, Future, 1, 2).Type.Should().Be(PenaltyType.Warning);

    [Fact]
    public void Create_SetsDuration() =>
        Penalty.Create(PenaltyType.Warning, Future, 1, 2).Duration.Should().Be(Future);

    [Fact]
    public void Create_SetsMatchId() =>
        Penalty.Create(PenaltyType.Warning, Future, 10, 2).MatchId.Should().Be(10);

    [Fact]
    public void Create_SetsUserId() =>
        Penalty.Create(PenaltyType.Warning, Future, 1, 20).UserId.Should().Be(20);

    [Fact]
    public void IsActive_PermanentBan_IsTrueEvenWithPastDuration() =>
        Penalty.Create(PenaltyType.PermanentBan, Past, 1, 2).IsActive().Should().BeTrue();

    [Fact]
    public void IsActive_TemporaryBanInFuture_IsTrue() =>
        Penalty.Create(PenaltyType.TemporaryBan, Future, 1, 2).IsActive().Should().BeTrue();

    [Fact]
    public void IsActive_TemporaryBanInPast_IsFalse() =>
        Penalty.Create(PenaltyType.TemporaryBan, Past, 1, 2).IsActive().Should().BeFalse();

    [Fact]
    public void IsActive_ExpiredWarning_IsFalse() =>
        Penalty.Create(PenaltyType.Warning, Past, 1, 2).IsActive().Should().BeFalse();
}
