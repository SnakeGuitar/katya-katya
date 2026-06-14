using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Social;
using Xunit;

namespace KatyaKatya.Domain.Tests.SocialTests;

public class SocialNetworkTests
{
    [Fact]
    public void Create_SetsUserId() =>
        SocialNetwork.Create(5, "@alice").UserId.Should().Be(5);

    [Fact]
    public void Create_SetsAccount() =>
        SocialNetwork.Create(5, "@alice").Account.Should().Be("@alice");

    [Fact]
    public void Create_EmptyAccount_Throws() =>
        ((Action)(() => SocialNetwork.Create(5, ""))).Should().Throw<DomainException>();

    [Fact]
    public void Create_AccountOver50Characters_Throws() =>
        ((Action)(() => SocialNetwork.Create(5, new string('x', 51)))).Should().Throw<DomainException>();

    [Fact]
    public void UpdateAccount_ChangesAccount()
    {
        var network = SocialNetwork.Create(5, "@alice");
        network.UpdateAccount("@bob");
        network.Account.Should().Be("@bob");
    }

    [Fact]
    public void UpdateAccount_Empty_Throws() =>
        ((Action)(() => SocialNetwork.Create(5, "@alice").UpdateAccount(""))).Should().Throw<DomainException>();
}
