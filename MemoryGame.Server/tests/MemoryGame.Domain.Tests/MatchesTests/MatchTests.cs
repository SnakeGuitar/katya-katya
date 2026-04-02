using MemoryGame.Domain.Common.Enums;
using MemoryGame.Domain.Matches;
using GameMatch = MemoryGame.Domain.Matches.Match;
using Xunit;
using MemoryGame.Domain.Common;

namespace MemoryGame.Tests;

public class MatchTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_StatusIsInProgress_ReturnNewMatch()
    {
        // Act
        GameMatch match = GameMatch.Create();

        // Assert
        Assert.Equal(MatchStatus.InProgress, match.Status);
    }


    // Method Finish()
    // Attribute validation tests.
    [Fact]
    public void Finish_WinnerIdIsValid_SetMatchStatusAsFinished()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;
        match.AddParticipant(userId);

        // Act
        match.Finish(userId);

        // Assert
        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    // Exception throw tests.
    [Fact]
    public void Finish_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int winnerUserId = 1;
        match.AddParticipant(winnerUserId);
        
        match.Finish(winnerUserId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Finish(2)
        );
    }

    [Fact]
    public void Finish_UserIdIsNotParticipant_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Finish(userId)
        );
    }


    // Method Cancel()
    // Attribute validation tests.
    [Fact]
    public void Cancel_CancelIsValid_SetMatchStatusAsCancelled()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        // Act
        match.Cancel();

        // Assert
        Assert.Equal(MatchStatus.Cancelled, match.Status);
    }

    // Exception throw tests.
    [Fact]
    public void Cancel_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        match.Cancel();

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Cancel()
        );
    }

    // Method AddParticipant()
    // Attribute validation tests.
    [Fact]
    public void AddParticipant_UserIdIsValid_ReturnNewMatchParticipation()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Act
        MatchParticipation participation = match.AddParticipant(userId);

        // Assert
        Assert.Equal(userId, participation.UserId);
    }

    [Fact]
    public void AddParticipant_MatchIdIsValid_ReturnNewMatchParticipation()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Act
        MatchParticipation participation = match.AddParticipant(userId);

        // Assert
        Assert.Equal(1, participation.MatchId);
    }

    // Exception throw tests.
    [Fact]
    public void AddParticipant_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int winnerUserId = 1;
        match.AddParticipant(winnerUserId);

        match.Finish(winnerUserId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.AddParticipant(2)
        );
    }

    [Fact]
    public void AddParticipant_ParticipantIsAlreadyInMatch_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;
        match.AddParticipant(userId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.AddParticipant(userId)
        );
    }
}