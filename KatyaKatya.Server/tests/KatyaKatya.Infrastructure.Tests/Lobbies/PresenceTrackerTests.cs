using FluentAssertions;
using KatyaKatya.Infrastructure.Lobbies;
using Xunit;

namespace KatyaKatya.Infrastructure.Tests.Lobbies;

public class PresenceTrackerTests
{
    [Fact]
    public void Track_MakesUserOnline()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.IsOnline(1).Should().BeTrue();
    }

    [Fact]
    public void Track_MapsUserToConnectionId()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.GetConnectionId(1).Should().Be("conn-1");
    }

    [Fact]
    public void Track_SameUserAgain_UpdatesConnectionId()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-old");
        tracker.Track(1, "conn-new");
        tracker.GetConnectionId(1).Should().Be("conn-new");
    }

    [Fact]
    public void IsOnline_UnknownUser_IsFalse() =>
        new PresenceTracker().IsOnline(999).Should().BeFalse();

    [Fact]
    public void GetConnectionId_UnknownUser_IsNull() =>
        new PresenceTracker().GetConnectionId(999).Should().BeNull();

    [Fact]
    public void Untrack_RemovesPresence()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.Untrack("conn-1");
        tracker.IsOnline(1).Should().BeFalse();
    }

    [Fact]
    public void Untrack_ClearsConnectionLookup()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.Untrack("conn-1");
        tracker.GetConnectionId(1).Should().BeNull();
    }

    [Fact]
    public void Untrack_UnknownConnection_LeavesOthersOnline()
    {
        var tracker = new PresenceTracker();
        tracker.Track(1, "conn-1");
        tracker.Untrack("conn-unknown");
        tracker.IsOnline(1).Should().BeTrue();
    }
}
