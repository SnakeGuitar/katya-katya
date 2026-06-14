using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Users;
using Xunit;

namespace KatyaKatya.Domain.Tests.UsersTests;

public class PendingRegistrationTests
{
    private static readonly TimeSpan Valid = TimeSpan.FromMinutes(15);

    private static PendingRegistration Create(TimeSpan? validity = null) =>
        PendingRegistration.Create("a@b.com", "alice", "123456", "hash", validity ?? Valid);

    [Fact]
    public void Create_SetsEmail() =>
        Create().Email.Value.Should().Be("a@b.com");

    [Fact]
    public void Create_SetsPin() =>
        Create().Pin.Should().Be("123456");

    [Fact]
    public void Create_SetsUsername() =>
        Create().Username.Should().Be("alice");

    [Fact]
    public void Create_SetsHashedPassword() =>
        Create().HashedPassword.Should().Be("hash");

    [Fact]
    public void Create_SetsFutureExpiration() =>
        Create().ExpirationTime.Should().BeAfter(DateTime.UtcNow);

    [Fact]
    public void Create_FreshRegistration_IsNotExpired() =>
        Create().IsExpired().Should().BeFalse();

    [Fact]
    public void Create_EmptyPin_Throws() =>
        ((Action)(() => PendingRegistration.Create("a@b.com", "u", "", "h", Valid))).Should().Throw<DomainException>();

    [Fact]
    public void Create_PinOver10Characters_Throws() =>
        ((Action)(() => PendingRegistration.Create("a@b.com", "u", "12345678901", "h", Valid))).Should().Throw<DomainException>();

    [Fact]
    public void Create_NegativeValidity_IsExpired() =>
        Create(TimeSpan.FromMinutes(-1)).IsExpired().Should().BeTrue();

    [Fact]
    public void UpdatePin_ChangesPin()
    {
        var reg = Create();
        reg.UpdatePin("654321");
        reg.Pin.Should().Be("654321");
    }

    [Fact]
    public void UpdatePin_Empty_Throws() =>
        ((Action)(() => Create().UpdatePin(""))).Should().Throw<DomainException>();

    [Fact]
    public void UpdatePin_TooLong_Throws() =>
        ((Action)(() => Create().UpdatePin("12345678901"))).Should().Throw<DomainException>();

    [Fact]
    public void ValidatePin_CorrectAndFresh_IsTrue() =>
        Create().ValidatePin("123456").Should().BeTrue();

    [Fact]
    public void ValidatePin_WrongPin_IsFalse() =>
        Create().ValidatePin("000000").Should().BeFalse();

    [Fact]
    public void ValidatePin_CorrectButExpired_IsFalse() =>
        Create(TimeSpan.FromMinutes(-1)).ValidatePin("123456").Should().BeFalse();
}
