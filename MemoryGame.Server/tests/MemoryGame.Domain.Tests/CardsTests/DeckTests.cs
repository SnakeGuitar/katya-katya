using MemoryGame.Domain.Cards;
using MemoryGame.Domain.Common;
using Xunit;

namespace MemoryGame.Tests;

public class DeckTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_NameIsValid_ReturnsNewDeck()
    {
        // Arrange
        string name = "Totally a real deck";
        int matchId = 1;

        // Act
        Deck deck = Deck.Create(name, matchId);

        // Assert
        Assert.Equal(name, deck.Name);
    }

    [Fact]
    public void Create_MatchIdIsValid_ReturnsNewDeck()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        // Act
        Deck deck = Deck.Create(name, matchId);

        // Assert
        Assert.Equal(matchId, deck.MatchId);
    }

    // Exception throw tests.
    [Fact]
    public void Create_NameIsNull_ThrowsDomainException()
    {
        // Arrange
        int matchId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Deck.Create(null, matchId)
        );
    }

    [Fact]
    public void Create_NameIsWhiteSpace_ThrowsDomainException()
    {
        // Arrange
        string name = " ";
        int matchId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Deck.Create(name, matchId)
        );
    }

    [Fact]
    public void Create_NameIsTooLong_ThrowsDomainException()
    {
        // Arrange
        string name = "I paused my imaginary OC AMV to be here (this better be good).";
        int matchId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Deck.Create(name, matchId)
        );
    }

    [Fact]
    public void Create_MatchIdIsNotValid_ThrowsDomainException()
    {
        // Arrange
        string name = "Totally real deck.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Deck.Create(name, 0)
        );
    }


    // Method AddCard()
    // Attribute validation tests.
    [Fact]
    public void AddCard_NameIsValid_ReturnsNewCard()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = "Chuck Norris";
        string cardDescription = "Can speak Braille.";

        // Act
        Card newCard = deck.AddCard(cardName, cardDescription);

        // Assert
        Assert.Equal(cardName, newCard.Name);
    }

    [Fact]
    public void AddCard_DescriptionIsValid_ReturnsNewCard()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = "Chuck Norris";
        string cardDescription = "Can speak Braille.";

        // Act
        Card newCard = deck.AddCard(cardName, cardDescription);

        // Assert
        Assert.Equal(cardDescription, newCard.Description);
    }

    [Fact]
    public void AddCard_DescriptionIsNull_ReturnsNewCard()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = "The Spy";

        // Act
        Card newCard = deck.AddCard(cardName, null);

        // Assert
        Assert.Null(newCard.Description);
    }

    // Exception throw tests.
    [Fact]
    public void AddCard_NameIsNull_ThrowsDomainException()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardDescription = "Can sp- Wait where did he go?";
        
        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            deck.AddCard(null, cardDescription)
        );
    }

    [Fact]
    public void AddCard_NameIsWhiteSpace_ThrowsDomainException()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = " ";
        string cardDescription = "Can sp- Not again.";
        
        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            deck.AddCard(cardName, cardDescription)
        );
    }

    [Fact]
    public void AddCard_NameIsTooLong_ThrowsDomainException()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = "MY ROFLCOPTER GOES SOI SOI SOI TCHE TCHE TCHE TCHE TCHE TCHE";
        string cardDescription = "Sample text.";
        
        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            deck.AddCard(cardName, cardDescription)
        );
    }

    [Fact]
    public void AddCard_DescriptionIsTooLong_ThrowsDomainException()
    {
        // Arrange
        string name = "Totally real deck.";
        int matchId = 1;

        Deck deck = Deck.Create(name, matchId);

        string cardName = "Halo";
        string cardDescription = "but mastar cheef is a pretty cool guy eh kills aleins and doesnt afraid of anything";
        
        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            deck.AddCard(cardName, cardDescription)
        );
    }
}