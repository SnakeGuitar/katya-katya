using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Matches.ValueObjects;
using Xunit;

namespace KatyaKatya.Domain.Tests.MatchesTests;

public class ScoreTests
{
    [Fact]
    public void Zero_HasValueZero() =>
        Score.Zero.Value.Should().Be(0);

    [Fact]
    public void Create_PositiveValue_SetsValue() =>
        Score.Create(42).Value.Should().Be(42);

    [Fact]
    public void Create_Zero_SetsValue() =>
        Score.Create(0).Value.Should().Be(0);

    [Fact]
    public void Create_NegativeValue_Throws() =>
        ((Action)(() => Score.Create(-1))).Should().Throw<DomainException>();

    [Fact]
    public void Add_IncreasesValue() =>
        Score.Create(10).Add(5).Value.Should().Be(15);

    [Fact]
    public void Add_ReturnsNewInstance()
    {
        var original = Score.Create(10);
        original.Add(5).Should().NotBeSameAs(original);
    }

    [Fact]
    public void Add_ResultingInNegative_Throws() =>
        ((Action)(() => Score.Zero.Add(-1))).Should().Throw<DomainException>();

    [Fact]
    public void Equals_SameValue_IsTrue() =>
        Score.Create(7).Equals(Score.Create(7)).Should().BeTrue();

    [Fact]
    public void Equals_DifferentValue_IsFalse() =>
        Score.Create(7).Equals(Score.Create(8)).Should().BeFalse();

    [Fact]
    public void GetHashCode_SameValue_IsEqual() =>
        Score.Create(7).GetHashCode().Should().Be(Score.Create(7).GetHashCode());

    [Fact]
    public void ToString_ReturnsValueString() =>
        Score.Create(7).ToString().Should().Be("7");
}
