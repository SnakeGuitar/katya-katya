using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Users;
using Xunit;

namespace KatyaKatya.Domain.Tests.UsersTests;

public class UserTests
{
    private static User Registered() => User.CreateRegistered("alice", "alice@example.com", "hash");

    [Fact]
    public void CreateRegistered_SetsUsername() =>
        Registered().Username.Should().Be("alice");

    [Fact]
    public void CreateRegistered_IsNotGuest() =>
        Registered().IsGuest.Should().BeFalse();

    [Fact]
    public void CreateRegistered_EmailStartsUnverified() =>
        Registered().VerifiedEmail.Should().BeFalse();

    [Fact]
    public void CreateRegistered_LowercasesEmail() =>
        User.CreateRegistered("a", "Alice@Example.com", "h").Email.Value.Should().Be("alice@example.com");

    [Fact]
    public void CreateRegistered_EmptyUsername_Throws() =>
        ((Action)(() => User.CreateRegistered("", "a@b.com", "h"))).Should().Throw<DomainException>();

    [Fact]
    public void CreateRegistered_UsernameOver30Characters_Throws() =>
        ((Action)(() => User.CreateRegistered(new string('x', 31), "a@b.com", "h"))).Should().Throw<DomainException>();

    [Fact]
    public void CreateGuest_IsGuest() =>
        User.CreateGuest("guest1").IsGuest.Should().BeTrue();

    [Fact]
    public void CreateGuest_GeneratesGuestEmail() =>
        User.CreateGuest("guest1").Email.Value.Should().Be("guest1@guest.memorygame");

    [Fact]
    public void ChangeUsername_UpdatesUsername()
    {
        var user = Registered();
        user.ChangeUsername("bob");
        user.Username.Should().Be("bob");
    }

    [Fact]
    public void ChangeUsername_Empty_Throws() =>
        ((Action)(() => Registered().ChangeUsername(""))).Should().Throw<DomainException>();

    [Fact]
    public void UpdatePersonalInfo_SetsName()
    {
        var user = Registered();
        user.UpdatePersonalInfo("Alice", "Smith");
        user.Name.Should().Be("Alice");
    }

    [Fact]
    public void UpdatePersonalInfo_SetsLastName()
    {
        var user = Registered();
        user.UpdatePersonalInfo("Alice", "Smith");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void UpdatePersonalInfo_NameOver50Characters_Throws() =>
        ((Action)(() => Registered().UpdatePersonalInfo(new string('n', 51), null))).Should().Throw<DomainException>();

    [Fact]
    public void UpdatePersonalInfo_LastNameOver50Characters_Throws() =>
        ((Action)(() => Registered().UpdatePersonalInfo(null, new string('l', 51)))).Should().Throw<DomainException>();

    [Fact]
    public void UpdateAvatar_SetsAvatar()
    {
        var user = Registered();
        var bytes = new byte[] { 1, 2, 3 };
        user.UpdateAvatar(bytes);
        user.Avatar.Should().BeSameAs(bytes);
    }

    [Fact]
    public void UpdateAvatar_Null_Throws() =>
        ((Action)(() => Registered().UpdateAvatar(null!))).Should().Throw<DomainException>();

    [Fact]
    public void ChangePassword_UpdatesHash()
    {
        var user = Registered();
        user.ChangePassword("newhash");
        user.PasswordHash.Should().Be("newhash");
    }

    [Fact]
    public void ChangePassword_OnGuest_Throws() =>
        ((Action)(() => User.CreateGuest("g").ChangePassword("h"))).Should().Throw<DomainException>();

    [Fact]
    public void VerifyEmail_SetsVerified()
    {
        var user = Registered();
        user.VerifyEmail();
        user.VerifiedEmail.Should().BeTrue();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_Throws()
    {
        var user = Registered();
        user.VerifyEmail();
        ((Action)user.VerifyEmail).Should().Throw<DomainException>();
    }

    [Fact]
    public void PromoteFromGuest_ClearsGuestFlag()
    {
        var user = User.CreateGuest("g");
        user.PromoteFromGuest("real@example.com", "hash");
        user.IsGuest.Should().BeFalse();
    }

    [Fact]
    public void PromoteFromGuest_SetsEmail()
    {
        var user = User.CreateGuest("g");
        user.PromoteFromGuest("real@example.com", "hash");
        user.Email.Value.Should().Be("real@example.com");
    }

    [Fact]
    public void PromoteFromGuest_OnRegisteredUser_Throws() =>
        ((Action)(() => Registered().PromoteFromGuest("x@y.com", "h"))).Should().Throw<DomainException>();
}
