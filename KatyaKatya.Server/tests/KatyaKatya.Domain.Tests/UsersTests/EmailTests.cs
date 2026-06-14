using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Users.ValueObjects;
using Xunit;

namespace KatyaKatya.Domain.Tests.UsersTests;

public class EmailTests
{
    [Fact]
    public void Create_ValidValue_KeepsLocalAndDomain() =>
        Email.Create("user@example.com").Value.Should().Be("user@example.com");

    [Fact]
    public void Create_UppercaseValue_IsLowercased() =>
        Email.Create("User@Example.COM").Value.Should().Be("user@example.com");

    [Fact]
    public void Create_ValueWithSurroundingWhitespace_IsTrimmed() =>
        Email.Create("  user@example.com  ").Value.Should().Be("user@example.com");

    [Fact]
    public void Create_EmptyValue_Throws() =>
        ((Action)(() => Email.Create(""))).Should().Throw<DomainException>();

    [Fact]
    public void Create_WhitespaceValue_Throws() =>
        ((Action)(() => Email.Create("   "))).Should().Throw<DomainException>();

    [Fact]
    public void Create_ValueOver50Characters_Throws() =>
        ((Action)(() => Email.Create(new string('a', 45) + "@b.com"))).Should().Throw<DomainException>();

    [Fact]
    public void Create_ValueWithoutAtSign_Throws() =>
        ((Action)(() => Email.Create("not-an-email"))).Should().Throw<DomainException>();

    [Fact]
    public void Equals_SameValue_IsTrue() =>
        Email.Create("a@b.com").Equals(Email.Create("a@b.com")).Should().BeTrue();

    [Fact]
    public void Equals_DifferentValue_IsFalse() =>
        Email.Create("a@b.com").Equals(Email.Create("c@d.com")).Should().BeFalse();

    [Fact]
    public void GetHashCode_SameValue_IsEqual() =>
        Email.Create("a@b.com").GetHashCode().Should().Be(Email.Create("a@b.com").GetHashCode());

    [Fact]
    public void ToString_ReturnsValue() =>
        Email.Create("a@b.com").ToString().Should().Be("a@b.com");
}
