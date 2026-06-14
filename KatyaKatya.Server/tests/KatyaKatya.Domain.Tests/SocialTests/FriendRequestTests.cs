using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Common.Enums;
using KatyaKatya.Domain.Social;
using Xunit;

namespace KatyaKatya.Domain.Tests.SocialTests;

public class FriendRequestTests
{
    [Fact]
    public void Create_StatusIsPending() =>
        FriendRequest.Create(1, 2).Status.Should().Be(FriendRequestStatus.Pending);

    [Fact]
    public void Create_SetsSenderId() =>
        FriendRequest.Create(1, 2).SenderId.Should().Be(1);

    [Fact]
    public void Create_SetsReceiverId() =>
        FriendRequest.Create(1, 2).ReceiverId.Should().Be(2);

    [Fact]
    public void Create_SetsSentAt() =>
        FriendRequest.Create(1, 2).SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

    [Fact]
    public void Create_SenderEqualsReceiver_Throws() =>
        ((Action)(() => FriendRequest.Create(1, 1))).Should().Throw<DomainException>();

    [Fact]
    public void Accept_SetsStatusAccepted()
    {
        var request = FriendRequest.Create(1, 2);
        request.Accept();
        request.Status.Should().Be(FriendRequestStatus.Accepted);
    }

    [Fact]
    public void Accept_WhenNotPending_Throws()
    {
        var request = FriendRequest.Create(1, 2);
        request.Accept();
        ((Action)request.Accept).Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_SetsStatusRejected()
    {
        var request = FriendRequest.Create(1, 2);
        request.Reject();
        request.Status.Should().Be(FriendRequestStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenNotPending_Throws()
    {
        var request = FriendRequest.Create(1, 2);
        request.Reject();
        ((Action)request.Reject).Should().Throw<DomainException>();
    }
}
