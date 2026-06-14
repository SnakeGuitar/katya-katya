using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Users;
using Xunit;

namespace KatyaKatya.Domain.Tests.UsersTests;

public class UserSessionTests
{
    private static UserSession Create(TimeSpan? duration = null) =>
        UserSession.Create("token-abc", 7, duration ?? TimeSpan.FromHours(1));

    [Fact]
    public void Create_SetsToken() =>
        Create().Token.Should().Be("token-abc");

    [Fact]
    public void Create_SetsUserId() =>
        Create().UserId.Should().Be(7);

    [Fact]
    public void Create_SetsFutureExpiry() =>
        Create().ExpiresAt.Should().BeAfter(DateTime.UtcNow);

    [Fact]
    public void Create_FreshSession_IsNotExpired() =>
        Create().IsExpired().Should().BeFalse();

    [Fact]
    public void Create_EmptyToken_Throws() =>
        ((Action)(() => UserSession.Create("", 1, TimeSpan.FromHours(1)))).Should().Throw<DomainException>();

    [Fact]
    public void Create_NegativeDuration_IsExpired() =>
        Create(TimeSpan.FromMinutes(-1)).IsExpired().Should().BeTrue();

    [Fact]
    public void Renew_ExtendsExpiry()
    {
        var session = Create(TimeSpan.FromMinutes(1));
        var before = session.ExpiresAt;
        session.Renew(TimeSpan.FromHours(2));
        session.ExpiresAt.Should().BeAfter(before);
    }

    [Fact]
    public void Renew_OnExpiredSession_MakesItValidAgain()
    {
        var session = Create(TimeSpan.FromMinutes(-1));
        session.Renew(TimeSpan.FromHours(1));
        session.IsExpired().Should().BeFalse();
    }
}
