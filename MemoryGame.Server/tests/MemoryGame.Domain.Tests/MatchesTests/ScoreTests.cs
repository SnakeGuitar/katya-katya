using MemoryGame.Domain.Common;
using MemoryGame.Domain.Matches.ValueObjects;
using Xunit;

namespace MemoryGame.Tests;

public class ScoreTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_ValueIsValid_ReturnNewScore()
    {
        // Arrange
        int value = 700;

        // Act
        Score score = Score.Create(value);

        // Assert
        Assert.Equal(value, score.Value);
    }

    // Exception throw tests.
    [Fact]
    public void Create_ValueIsNotValid_ThrowDomainException()
    {
        // Arrange
        int value = -40000;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Score.Create(value)
        );
    }


    // Method Add()
    // Attribute validation tests.
    [Fact]
    public void Add_PointsAreValid_ReturnUpdatedScore()
    {
        // Arrange
        Score score = Score.Create(700);

        // Act
        score = score.Add(50);

        // Assert
        Assert.Equal(750, score.Value);
    }

    // Exception throw tests.
    [Fact]
    public void Add_PointsAreNotValid_ThrowDomainException()
    {
        // Arrange
        Score score = Score.Create(700);
        int points = -50;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            score.Add(points)
        );
    }
}