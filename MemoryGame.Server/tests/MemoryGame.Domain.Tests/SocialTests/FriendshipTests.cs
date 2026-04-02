using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;
using Xunit;

namespace MemoryGame.Tests;

public class FriendshipTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_UserIdIsValid_ReturnNewFriendship()
    {
        // Arrange
        int userId = 1;
        int friendId = 2;

        // Act
        Friendship friendship = Friendship.Create(userId, friendId);

        // Assert
        Assert.Equal(userId, friendship.UserId);
    }

    [Fact]
    public void Create_FriendIdIsValid_ReturnNewFriendship()
    {
        // Arrange
        int userId = 1;
        int friendId = 2;

        // Act
        Friendship friendship = Friendship.Create(userId, friendId);

        // Assert
        Assert.Equal(friendId, friendship.FriendId);
    }

    // Exception throw tests.
    [Fact]
    public void Create_IdsAreTheSame_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        int friendId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Friendship.Create(userId, friendId)
        );
    }

    [Fact]
    public void Create_UserIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        int userId = -1;
        int friendId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Friendship.Create(userId, friendId)
        );
    }

    [Fact]
    public void Create_FriendIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        int friendId = -1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Friendship.Create(userId, friendId)
        );
    }
}