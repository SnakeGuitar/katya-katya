using FluentAssertions;
using MemoryGame.Application.Lobbies.DTOs;
using MemoryGame.Application.Lobbies.Models;
using Xunit;

namespace MemoryGame.Application.Tests.Lobbies;

public class LobbyTests
{
    private static LobbyPlayer MakePlayer(string connectionId, int userId, string username, bool isHost = false)
        => new(connectionId, userId, username, isGuest: false, isHost);

    // -----------------------------------------------------------------------
    // TryAddPlayer
    // -----------------------------------------------------------------------

    [Fact]
    public void TryAddPlayer_WhenUnderCapacity_ReturnsTrue()
    {
        var lobby = new Lobby("ABCD", isPublic: true);

        var result = lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));

        result.Should().BeTrue();
        lobby.Players.Should().ContainKey("conn-1");
    }

    [Fact]
    public void TryAddPlayer_WhenFull_ReturnsFalse()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        for (var i = 0; i < Lobby.MaxPlayers; i++)
            lobby.TryAddPlayer(MakePlayer($"conn-{i}", i, $"player{i}"));

        var result = lobby.TryAddPlayer(MakePlayer("conn-x", 99, "extra"));

        result.Should().BeFalse();
    }

    [Fact]
    public void TryAddPlayer_DuplicateConnectionId_ReturnsFalse()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice"));

        var result = lobby.TryAddPlayer(MakePlayer("conn-1", 2, "bob"));

        result.Should().BeFalse();
        lobby.Players.Should().HaveCount(1);
    }

    // -----------------------------------------------------------------------
    // RemovePlayer
    // -----------------------------------------------------------------------

    [Fact]
    public void RemovePlayer_ExistingPlayer_ReturnsPlayer()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice"));

        var removed = lobby.RemovePlayer("conn-1");

        removed.Should().NotBeNull();
        removed!.Username.Should().Be("alice");
        lobby.Players.Should().BeEmpty();
    }

    [Fact]
    public void RemovePlayer_NonExistingConnection_ReturnsNull()
    {
        var lobby = new Lobby("ABCD", isPublic: true);

        var removed = lobby.RemovePlayer("unknown");

        removed.Should().BeNull();
    }

    [Fact]
    public void RemovePlayer_HostLeaves_PromotesOldestRemainingPlayer()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        lobby.RemovePlayer("conn-1");

        lobby.GetHost()!.Username.Should().Be("bob");
    }

    [Fact]
    public void RemovePlayer_NonHost_DoesNotChangeHost()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        lobby.RemovePlayer("conn-2");

        lobby.GetHost()!.Username.Should().Be("alice");
    }

    // -----------------------------------------------------------------------
    // VoteToKick
    // -----------------------------------------------------------------------

    [Fact]
    public void VoteToKick_BelowMajorityThreshold_ReturnsFalse()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));
        lobby.TryAddPlayer(MakePlayer("conn-3", 3, "charlie"));

        var result = lobby.VoteToKick("alice", "charlie");

        result.Should().BeFalse();
    }

    [Fact]
    public void VoteToKick_MajorityReached_ReturnsTrue()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));
        lobby.TryAddPlayer(MakePlayer("conn-3", 3, "charlie"));

        lobby.VoteToKick("alice", "charlie");
        var result = lobby.VoteToKick("bob", "charlie");

        result.Should().BeTrue();
    }

    [Fact]
    public void VoteToKick_SameVoterTwice_CountsOnce()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        lobby.VoteToKick("alice", "bob");
        var result = lobby.VoteToKick("alice", "bob");

        result.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // StartGame
    // -----------------------------------------------------------------------

    [Fact]
    public void StartGame_CreatesGameSession_WithCorrectPlayerOrder()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        var game = lobby.StartGame(new GameSettingsDto(CardCount: 8, TurnTimeSeconds: 30));

        game.Should().NotBeNull();
        game.TurnOrder.Should().StartWith("alice");
        game.TurnOrder.Should().Contain("bob");
    }

    [Fact]
    public void StartGame_SetsIsGameInProgress()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        lobby.StartGame(new GameSettingsDto(CardCount: 8, TurnTimeSeconds: 30));

        lobby.IsGameInProgress.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // GetPlayerList
    // -----------------------------------------------------------------------

    [Fact]
    public void GetPlayerList_ReturnsAllPlayers_AsDto()
    {
        var lobby = new Lobby("ABCD", isPublic: true);
        lobby.TryAddPlayer(MakePlayer("conn-1", 1, "alice", isHost: true));
        lobby.TryAddPlayer(MakePlayer("conn-2", 2, "bob"));

        var list = lobby.GetPlayerList();

        list.Should().HaveCount(2);
        list.Should().Contain(p => p.Username == "alice" && p.IsHost);
        list.Should().Contain(p => p.Username == "bob" && !p.IsHost);
    }

    // -----------------------------------------------------------------------
    // IsGameInProgress
    // -----------------------------------------------------------------------

    [Fact]
    public void IsGameInProgress_WhenNoGameStarted_ReturnsFalse()
    {
        var lobby = new Lobby("ABCD", isPublic: true);

        lobby.IsGameInProgress.Should().BeFalse();
    }
}
