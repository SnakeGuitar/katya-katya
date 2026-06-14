using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Social;
using Xunit;

namespace KatyaKatya.Domain.Tests.SocialTests;

public class FriendshipTests
{
    [Fact]
    public void Create_SetsUserId() =>
        Friendship.Create(1, 2).UserId.Should().Be(1);

    [Fact]
    public void Create_SetsFriendId() =>
        Friendship.Create(1, 2).FriendId.Should().Be(2);

    [Fact]
    public void Create_SetsCreatedAt() =>
        Friendship.Create(1, 2).CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

    [Fact]
    public void Create_SelfFriendship_Throws() =>
        ((Action)(() => Friendship.Create(1, 1))).Should().Throw<DomainException>();
}
