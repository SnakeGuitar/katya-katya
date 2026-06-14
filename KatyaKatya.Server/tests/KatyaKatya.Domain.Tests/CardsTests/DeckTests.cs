using FluentAssertions;
using KatyaKatya.Domain.Cards;
using KatyaKatya.Domain.Common;
using Xunit;

namespace KatyaKatya.Domain.Tests.CardsTests;

public class DeckTests
{
    [Fact]
    public void Create_SetsName() =>
        Deck.Create("Main", 1).Name.Should().Be("Main");

    [Fact]
    public void Create_SetsMatchId() =>
        Deck.Create("Main", 9).MatchId.Should().Be(9);

    [Fact]
    public void Create_HasNoCards() =>
        Deck.Create("Main", 1).Cards.Should().BeEmpty();

    [Fact]
    public void Create_EmptyName_Throws() =>
        ((Action)(() => Deck.Create("", 1))).Should().Throw<DomainException>();

    [Fact]
    public void Create_NameOver30Characters_Throws() =>
        ((Action)(() => Deck.Create(new string('n', 31), 1))).Should().Throw<DomainException>();

    [Fact]
    public void AddCard_AddsToCards()
    {
        var deck = Deck.Create("Main", 1);
        deck.AddCard("Ace");
        deck.Cards.Should().HaveCount(1);
    }

    [Fact]
    public void AddCard_ReturnsCardWithName() =>
        Deck.Create("Main", 1).AddCard("Ace").Name.Should().Be("Ace");

    [Fact]
    public void AddCard_InvalidName_Throws() =>
        ((Action)(() => Deck.Create("Main", 1).AddCard(""))).Should().Throw<DomainException>();
}
