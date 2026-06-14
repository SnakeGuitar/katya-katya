using FluentAssertions;
using KatyaKatya.Domain.Cards;
using KatyaKatya.Domain.Common;
using Xunit;

namespace KatyaKatya.Domain.Tests.CardsTests;

public class CardTests
{
    [Fact]
    public void Create_SetsName() =>
        Card.Create("Ace", 1).Name.Should().Be("Ace");

    [Fact]
    public void Create_SetsDeckId() =>
        Card.Create("Ace", 7).DeckId.Should().Be(7);

    [Fact]
    public void Create_SetsDescription() =>
        Card.Create("Ace", 1, "high card").Description.Should().Be("high card");

    [Fact]
    public void Create_WithoutDescription_LeavesItNull() =>
        Card.Create("Ace", 1).Description.Should().BeNull();

    [Fact]
    public void Create_EmptyName_Throws() =>
        ((Action)(() => Card.Create("", 1))).Should().Throw<DomainException>();

    [Fact]
    public void Create_NameOver30Characters_Throws() =>
        ((Action)(() => Card.Create(new string('n', 31), 1))).Should().Throw<DomainException>();

    [Fact]
    public void Create_DescriptionOver80Characters_Throws() =>
        ((Action)(() => Card.Create("Ace", 1, new string('d', 81)))).Should().Throw<DomainException>();
}
