using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;
using MemoryGame.Domain.Penalties;
using Xunit;

namespace MemoryGame.Tests;

public class PenaltyTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_PenaltyTypeIsValid_ReturnNewPenalty()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.Equal(type, penalty.Type);
    }

    [Fact]
    public void Create_DurationIsValid_ReturnNewPenalty()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.Equal(duration, penalty.Duration);
    }

    [Fact]
    public void Create_MatchIdIsValid_ReturnNewPenalty()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.Equal(matchId, penalty.MatchId);
    }

    [Fact]
    public void Create_UserIdIsValid_ReturnNewPenalty()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.Equal(userId, penalty.UserId);
    }

    // Exception throw tests.
    [Fact]
    public void Create_DurationIsBeforePresentTime_ThrowDomainException()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = DateTime.Now.AddDays(-1);
        int matchId = -1;
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Penalty.Create(type, duration, matchId, userId)
        );
    }

    [Fact]
    public void Create_MatchIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = -1;
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Penalty.Create(type, duration, matchId, userId)
        );
    }

    [Fact]
    public void Create_UserIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = -1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Penalty.Create(type, duration, matchId, userId)
        );
    }


    // Method IsActive()
    // Attribute validation tests.
    [Fact]
    public void IsActive_PenaltyTypeIsPermanentBan_ReturnsTrue()
    {
        // Arrange
        PenaltyType type = PenaltyType.PermanentBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.True(penalty.IsActive());
    }

    [Fact]
    public void IsActive_DurationIsFutureTime_ReturnsTrue()
    {
        // Arrange
        PenaltyType type = PenaltyType.TemporaryBan;
        DateTime duration = new DateTime(9999, 11, 30, 23, 59, 59);
        int matchId = 1;
        int userId = 1;

        // Act
        Penalty penalty = Penalty.Create(type, duration, matchId, userId);

        // Assert
        Assert.True(penalty.IsActive());
    }
}