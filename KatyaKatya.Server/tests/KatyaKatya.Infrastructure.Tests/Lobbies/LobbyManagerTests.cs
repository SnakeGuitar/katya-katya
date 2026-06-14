using FluentAssertions;
using KatyaKatya.Application.Lobbies.DTOs;
using KatyaKatya.Application.Lobbies.Models;
using KatyaKatya.Infrastructure.Lobbies;
using Xunit;

namespace KatyaKatya.Infrastructure.Tests.Lobbies;

public class LobbyManagerTests
{
    private static LobbyPlayer Player(string conn, int userId, string name, bool isHost = false) =>
        new(conn, userId, name, isGuest: false, isHost: isHost);

    [Fact]
    public void CreateLobby_ReturnsLobby() =>
        new LobbyManager().CreateLobby("ABC", isPublic: true).Should().NotBeNull();

    [Fact]
    public void CreateLobby_SetsGameCode() =>
        new LobbyManager().CreateLobby("ABC", true)!.GameCode.Should().Be("ABC");

    [Fact]
    public void CreateLobby_DuplicateCode_ReturnsNull()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABC", true);
        manager.CreateLobby("ABC", true).Should().BeNull();
    }

    [Fact]
    public void GetLobby_ExistingCode_ReturnsLobby()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABC", true);
        manager.GetLobby("ABC").Should().NotBeNull();
    }

    [Fact]
    public void GetLobby_UnknownCode_ReturnsNull() =>
        new LobbyManager().GetLobby("NOPE").Should().BeNull();

    [Fact]
    public void RemoveLobby_ExistingCode_ReturnsTrue()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABC", true);
        manager.RemoveLobby("ABC").Should().BeTrue();
    }

    [Fact]
    public void RemoveLobby_UnknownCode_ReturnsFalse() =>
        new LobbyManager().RemoveLobby("NOPE").Should().BeFalse();

    [Fact]
    public void RemoveLobby_ActuallyRemovesLobby()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABC", true);
        manager.RemoveLobby("ABC");
        manager.GetLobby("ABC").Should().BeNull();
    }

    [Fact]
    public void GetPublicLobbies_IncludesAvailablePublicLobby()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("PUB", isPublic: true);
        manager.GetPublicLobbies().Should().ContainSingle(s => s.GameCode == "PUB");
    }

    [Fact]
    public void GetPublicLobbies_ExcludesPrivateLobby()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("PRIV", isPublic: false);
        manager.GetPublicLobbies().Should().NotContain(s => s.GameCode == "PRIV");
    }

    [Fact]
    public void GetPublicLobbies_ExcludesFullLobby()
    {
        var manager = new LobbyManager();
        var lobby = manager.CreateLobby("FULL", isPublic: true)!;
        for (var i = 0; i < Lobby.MaxPlayers; i++)
            lobby.TryAddPlayer(Player($"c{i}", i + 1, $"p{i}"));
        manager.GetPublicLobbies().Should().NotContain(s => s.GameCode == "FULL");
    }

    [Fact]
    public void GetPublicLobbies_ExcludesLobbyWithGameInProgress()
    {
        var manager = new LobbyManager();
        var lobby = manager.CreateLobby("PLAYING", isPublic: true)!;
        lobby.TryAddPlayer(Player("c1", 1, "alice", isHost: true));
        lobby.StartGame(new GameSettingsDto(4, 30));
        manager.GetPublicLobbies().Should().NotContain(s => s.GameCode == "PLAYING");
    }

    [Fact]
    public void FindLobbyByConnection_ReturnsLobbyContainingConnection()
    {
        var manager = new LobbyManager();
        var lobby = manager.CreateLobby("ABC", true)!;
        lobby.TryAddPlayer(Player("conn-1", 1, "alice", isHost: true));
        manager.FindLobbyByConnection("conn-1")!.GameCode.Should().Be("ABC");
    }

    [Fact]
    public void FindLobbyByConnection_UnknownConnection_ReturnsNull() =>
        new LobbyManager().FindLobbyByConnection("nope").Should().BeNull();

    [Fact]
    public void FindLobbyByUserId_ReturnsLobbyContainingUser()
    {
        var manager = new LobbyManager();
        var lobby = manager.CreateLobby("ABC", true)!;
        lobby.TryAddPlayer(Player("conn-1", 42, "alice", isHost: true));
        manager.FindLobbyByUserId(42)!.GameCode.Should().Be("ABC");
    }

    [Fact]
    public void FindLobbyByUserId_NonPositiveId_ReturnsNull() =>
        new LobbyManager().FindLobbyByUserId(0).Should().BeNull();

    [Fact]
    public void FindLobbyByUserId_UnknownUser_ReturnsNull() =>
        new LobbyManager().FindLobbyByUserId(999).Should().BeNull();
}
