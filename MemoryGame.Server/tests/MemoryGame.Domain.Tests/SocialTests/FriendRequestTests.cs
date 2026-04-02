using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;
using MemoryGame.Domain.Social;
using Xunit;

namespace MemoryGame.Tests;

public class FriendRequestTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_SenderIdIsValid_ReturnNewFriendRequest()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        // Act
        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        // Assert
        Assert.Equal(senderId, friendRequest.SenderId);
    }

    [Fact]
    public void Create_ReceiverIdIsValid_ReturnNewFriendRequest()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        // Act
        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        // Assert
        Assert.Equal(receiverId, friendRequest.ReceiverId);
    }

    // Exception throw tests.
    [Fact]
    public void Create_IdsAreTheSame_ThrowDomainException()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            FriendRequest.Create(senderId, receiverId)
        );
    }

    [Fact]
    public void Create_SenderIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        int senderId = -1;
        int receiverId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            FriendRequest.Create(senderId, receiverId)
        );
    }

    [Fact]
    public void Create_ReceiverIdIsNotValid_ThrowDomainException()
    {
        // Arrange
        int senderId = 1;
        int receiverId = -1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            FriendRequest.Create(senderId, receiverId)
        );
    }


    // Method Accept()
    // Attribute validation tests.
    [Fact]
    public void Accept_RequestStatusIsPending_SetRequestStatusAsAccepted()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        // Act
        friendRequest.Accept();

        // Assert
        Assert.Equal(FriendRequestStatus.Accepted, friendRequest.Status);
    }

    // Exception throw tests.
    [Fact]
    public void Accept_RequestStatusIsNotPending_ThrowDomainException()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        friendRequest.Accept();

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            friendRequest.Accept()
        );
    }


    // Method Reject()
    // Attribute validation tests.
    [Fact]
    public void Reject_RequestStatusIsPending_SetRequestStatusAsRejected()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        // Act
        friendRequest.Reject();

        // Assert
        Assert.Equal(FriendRequestStatus.Rejected, friendRequest.Status);
    }

    // Exception throw tests.
    [Fact]
    public void Reject_RequestStatusIsNotPending_ThrowDomainException()
    {
        // Arrange
        int senderId = 1;
        int receiverId = 2;

        FriendRequest friendRequest = FriendRequest.Create(senderId, receiverId);

        friendRequest.Reject();

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            friendRequest.Reject()
        );
    }
}