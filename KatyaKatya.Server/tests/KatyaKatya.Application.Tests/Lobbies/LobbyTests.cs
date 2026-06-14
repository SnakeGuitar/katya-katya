using FluentAssertions;
using KatyaKatya.Application.Lobbies.DTOs;
using KatyaKatya.Application.Lobbies.Models;
using Xunit;

namespace KatyaKatya.Application.Tests.Lobbies;

public class LobbyTests
{
    private static Lobby Lobby() => new("ABC123", isPublic: true);

    private static LobbyPlayer Player(string conn, string name, bool isHost = false) =>
        new(conn, userId: name.GetHashCode(), username: name, isGuest: false, isHost: isHost);

    [Fact]
    public void Constructor_SetsGameCode() =>
        Lobby().GameCode.Should().Be("ABC123");

    [Fact]
    public void Constructor_SetsIsPublic() =>
        Lobby().IsPublic.Should().BeTrue();

    [Fact]
    public void Constructor_GameStartsNull() =>
        Lobby().Game.Should().BeNull();

    [Fact]
    public void Constructor_GameNotInProgress() =>
        Lobby().IsGameInProgress.Should().BeFalse();

    [Fact]
    public void TryAddPlayer_AddsPlayer()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.Players.Should().HaveCount(1);
    }

    [Fact]
    public void TryAddPlayer_WithinCapacity_ReturnsTrue() =>
        Lobby().TryAddPlayer(Player("c1", "alice", true)).Should().BeTrue();

    [Fact]
    public void TryAddPlayer_WhenFull_ReturnsFalse()
    {
        var lobby = Lobby();
        for (var i = 0; i < Models_MaxPlayers(); i++)
            lobby.TryAddPlayer(Player($"c{i}", $"p{i}"));
        lobby.TryAddPlayer(Player("overflow", "extra")).Should().BeFalse();
    }

    [Fact]
    public void GetPlayer_ReturnsPlayerByConnectionId()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.GetPlayer("c1")!.Username.Should().Be("alice");
    }

    [Fact]
    public void GetPlayer_UnknownConnectionId_ReturnsNull() =>
        Lobby().GetPlayer("nope").Should().BeNull();

    [Fact]
    public void GetHost_ReturnsTheHost()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.GetHost()!.Username.Should().Be("alice");
    }

    [Fact]
    public void RemovePlayer_ReturnsRemovedPlayer()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.RemovePlayer("c1")!.Username.Should().Be("alice");
    }

    [Fact]
    public void RemovePlayer_UnknownConnectionId_ReturnsNull() =>
        Lobby().RemovePlayer("nope").Should().BeNull();

    [Fact]
    public void RemovePlayer_WhenHostLeaves_PromotesRemainingPlayerToHost()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.TryAddPlayer(Player("c2", "bob"));
        lobby.RemovePlayer("c1");
        lobby.GetPlayer("c2")!.IsHost.Should().BeTrue();
    }

    [Fact]
    public void StartGame_SetsGame()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.StartGame(new GameSettingsDto(4, 30));
        lobby.Game.Should().NotBeNull();
    }

    [Fact]
    public void StartGame_CreatesBoardWithRequestedCardCount()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.StartGame(new GameSettingsDto(6, 30)).Board.Should().HaveCount(6);
    }

    [Fact]
    public void StartGame_MarksGameInProgress()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.StartGame(new GameSettingsDto(4, 30));
        lobby.IsGameInProgress.Should().BeTrue();
    }

    [Fact]
    public void VoteToKick_BelowThreshold_ReturnsFalse()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.TryAddPlayer(Player("c2", "bob"));
        lobby.VoteToKick("alice", "bob").Should().BeFalse();
    }

    [Fact]
    public void VoteToKick_ReachingMajority_ReturnsTrue()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.TryAddPlayer(Player("c2", "bob"));
        lobby.VoteToKick("alice", "carol");
        lobby.VoteToKick("bob", "carol").Should().BeTrue();
    }

    [Fact]
    public void ToSummary_MapsGameCode()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.ToSummary().GameCode.Should().Be("ABC123");
    }

    [Fact]
    public void ToSummary_CountsCurrentPlayers()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.TryAddPlayer(Player("c2", "bob"));
        lobby.ToSummary().CurrentPlayers.Should().Be(2);
    }

    [Fact]
    public void GetPlayerList_ReturnsAllPlayers()
    {
        var lobby = Lobby();
        lobby.TryAddPlayer(Player("c1", "alice", true));
        lobby.TryAddPlayer(Player("c2", "bob"));
        lobby.GetPlayerList().Should().HaveCount(2);
    }

    private static int Models_MaxPlayers() => KatyaKatya.Application.Lobbies.Models.Lobby.MaxPlayers;
}
