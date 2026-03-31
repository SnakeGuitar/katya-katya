using FluentAssertions;
using MemoryGame.Infrastructure.Lobbies;
using Xunit;

namespace MemoryGame.Infrastructure.Tests.Lobbies;

public class PresenceTrackerTests
{
    // -----------------------------------------------------------------------
    // Track / IsOnline
    // -----------------------------------------------------------------------

    [Fact]
    public void Track_NewUser_IsOnline()
    {
        var tracker = new PresenceTracker();

        tracker.Track(userId: 1, connectionId: "conn-abc");

        tracker.IsOnline(1).Should().BeTrue();
    }

    [Fact]
    public void Track_SameUser_SecondDevice_OverwritesConnectionId()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-old");

        tracker.Track(1, "conn-new");

        tracker.GetConnectionId(1).Should().Be("conn-new");
    }

    [Fact]
    public void Track_MultipleUsers_EachHasOwnConnectionId()
    {
        var tracker = new PresenceTracker();

        tracker.Track(1, "conn-a");
        tracker.Track(2, "conn-b");

        tracker.GetConnectionId(1).Should().Be("conn-a");
        tracker.GetConnectionId(2).Should().Be("conn-b");
    }

    // -----------------------------------------------------------------------
    // Untrack
    // -----------------------------------------------------------------------

    [Fact]
    public void Untrack_KnownConnection_UserGoesOffline()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-abc");

        tracker.Untrack("conn-abc");

        tracker.IsOnline(1).Should().BeFalse();
        tracker.GetConnectionId(1).Should().BeNull();
    }

    [Fact]
    public void Untrack_UnknownConnection_DoesNotThrow()
    {
        var tracker = new PresenceTracker();

        var act = () => tracker.Untrack("non-existent");

        act.Should().NotThrow();
    }

    [Fact]
    public void Untrack_DoesNotAffectOtherUsers()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.Track(2, "conn-2");

        tracker.Untrack("conn-1");

        tracker.IsOnline(2).Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // GetConnectionId
    // -----------------------------------------------------------------------

    [Fact]
    public void GetConnectionId_OnlineUser_ReturnsConnectionId()
    {
        var tracker = new PresenceTracker();
        tracker.Track(42, "conn-xyz");

        var connectionId = tracker.GetConnectionId(42);

        connectionId.Should().Be("conn-xyz");
    }

    [Fact]
    public void GetConnectionId_OfflineUser_ReturnsNull()
    {
        var tracker = new PresenceTracker();

        var connectionId = tracker.GetConnectionId(99);

        connectionId.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // IsOnline
    // -----------------------------------------------------------------------

    [Fact]
    public void IsOnline_UntrackedUser_ReturnsFalse()
    {
        var tracker = new PresenceTracker();

        tracker.IsOnline(7).Should().BeFalse();
    }

    [Fact]
    public void IsOnline_AfterUntrack_ReturnsFalse()
    {
        var tracker = new PresenceTracker();
        tracker.Track(7, "conn-7");

        tracker.Untrack("conn-7");

        tracker.IsOnline(7).Should().BeFalse();
    }
}
