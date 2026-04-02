using MemoryGame.Domain.Cards;
using MemoryGame.Domain.Common;
using Xunit;

namespace MemoryGame.Tests;

public class CardTests
{
    // Attribute validation tests.
    [Fact]
    public void Create_NameIsValid_ReturnsNewCard()
    {
        // Arrange
        string name = "John Doe";
        int deckId = 1;
        string description = "Sample text.";

        // Act
        Card card = Card.Create(name, deckId, description);

        // Assert
        Assert.Equal(name, card.Name);
    }

    [Fact]
    public void Create_DeckIdIsValid_ReturnsNewCard()
    {
        // Arrange
        string name = "John Doe";
        int deckId = 1;
        string description = "Sample text.";

        // Act
        Card card = Card.Create(name, deckId, description);

        // Assert
        Assert.Equal(deckId, card.DeckId);
    }

    [Fact]
    public void Create_DescriptionIsValid_ReturnsNewCard()
    {
        // Arrange
        string name = "John Doe";
        int deckId = 1;
        string description = "Sample text.";

        // Act
        Card card = Card.Create(name, deckId, description);

        // Assert
        Assert.Equal(description, card.Description);
    }

    [Fact]
    public void Create_DescriptionIsNull_ReturnsNewCard()
    {
        // Arrange
        string name = "John Doe";
        int deckId = 1;

        // Act
        Card card = Card.Create(name, deckId, null);

        // Assert
        Assert.Null(card.Description);
    }

    // Exception throw tests.
    [Fact]
    public void Create_NameIsNull_ThrowsDomainException()
    {
        // Arrange
        int deckId = 1;
        string description = "Sample text.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Card.Create(null, deckId, description)
        );
    }

    [Fact]
    public void Create_NameIsWhiteSpace_ThrowsDomainException()
    {
        // Arrange
        string name = " ";
        int deckId = 1;
        string description = "Sample text.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Card.Create(name, deckId, description)
        );
    }

    [Fact]
    public void Create_NameIsTooLong_ThrowsDomainException()
    {
        // Arrange
        string name = "I AM THE KING OF SPACE I HAVE COME TO SURVEY YOUR INTERNETS";
        int deckId = 1;
        string description = "Sample text.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Card.Create(name, deckId, description)
        );
    }

    [Fact]
    public void Create_DeckIdIsNotValid_ThrowsDomainException()
    {
        // Arrange
        string name = "John Doe";
        string description = "Sample text.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Card.Create(name, 0, description)
        );
    }

    [Fact]
    public void Create_DescriptionIsTooLong_ThrowsDomainException()
    {
        // Arrange
        string name = "John Doe";
        int deckId = 1;
        string description = "The industrial revolution and it's consecuences have been a disaster for the human race.";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Card.Create(name, deckId, description)
        );
    }

}