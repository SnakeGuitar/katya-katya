using FluentAssertions;
using MemoryGame.Application.Lobbies.Models;
using MemoryGame.Infrastructure.Lobbies;
using Xunit;

namespace MemoryGame.Infrastructure.Tests.Lobbies;

public class LobbyManagerTests
{
    private static LobbyPlayer MakeHost(string connectionId)
        => new(connectionId, userId: 1, username: "alice", isGuest: false, isHost: true);

    // -----------------------------------------------------------------------
    // CreateLobby
    // -----------------------------------------------------------------------

    [Fact]
    public void CreateLobby_NewCode_ReturnsLobby()
    {
        var manager = new LobbyManager();

        var lobby = manager.CreateLobby("ABCD", isPublic: true);

        lobby.Should().NotBeNull();
        lobby!.GameCode.Should().Be("ABCD");
    }

    [Fact]
    public void CreateLobby_DuplicateCode_ReturnsNull()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABCD", isPublic: true);

        var second = manager.CreateLobby("ABCD", isPublic: false);

        second.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GetLobby
    // -----------------------------------------------------------------------

    [Fact]
    public void GetLobby_ExistingCode_ReturnsLobby()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABCD", isPublic: true);

        var lobby = manager.GetLobby("ABCD");

        lobby.Should().NotBeNull();
    }

    [Fact]
    public void GetLobby_NonExistingCode_ReturnsNull()
    {
        var manager = new LobbyManager();

        var lobby = manager.GetLobby("XXXX");

        lobby.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // RemoveLobby
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveLobby_ExistingCode_ReturnsTrueAndRemoves()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABCD", isPublic: true);

        var result = manager.RemoveLobby("ABCD");

        result.Should().BeTrue();
        manager.GetLobby("ABCD").Should().BeNull();
    }

    [Fact]
    public void RemoveLobby_NonExistingCode_ReturnsFalse()
    {
        var manager = new LobbyManager();

        var result = manager.RemoveLobby("XXXX");

        result.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // FindLobbyByConnection
    // -----------------------------------------------------------------------

    [Fact]
    public void FindLobbyByConnection_KnownConnectionId_ReturnsCorrectLobby()
    {
        var manager = new LobbyManager();
        var lobby   = manager.CreateLobby("ABCD", isPublic: true)!;
        lobby.TryAddPlayer(MakeHost("conn-1"));

        var found = manager.FindLobbyByConnection("conn-1");

        found.Should().Be(lobby);
    }

    [Fact]
    public void FindLobbyByConnection_UnknownConnectionId_ReturnsNull()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("ABCD", isPublic: true);

        var found = manager.FindLobbyByConnection("unknown");

        found.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GetPublicLobbies
    // -----------------------------------------------------------------------

    [Fact]
    public void GetPublicLobbies_ExcludesPrivateLobbies()
    {
        var manager = new LobbyManager();
        manager.CreateLobby("PUB1", isPublic: true)!.TryAddPlayer(MakeHost("c1"));
        manager.CreateLobby("PRI1", isPublic: false)!.TryAddPlayer(MakeHost("c2"));

        var result = manager.GetPublicLobbies();

        result.Should().ContainSingle(l => l.GameCode == "PUB1");
        result.Should().NotContain(l => l.GameCode == "PRI1");
    }

    [Fact]
    public void GetPublicLobbies_ExcludesFullLobbies()
    {
        var manager = new LobbyManager();
        var lobby   = manager.CreateLobby("FULL", isPublic: true)!;

        for (var i = 0; i < Lobby.MaxPlayers; i++)
            lobby.TryAddPlayer(new LobbyPlayer($"conn-{i}", i, $"player{i}", false, i == 0));

        var result = manager.GetPublicLobbies();

        result.Should().NotContain(l => l.GameCode == "FULL");
    }

    [Fact]
    public void GetPublicLobbies_ExcludesLobbiesWithGameInProgress()
    {
        var manager = new LobbyManager();
        var lobby   = manager.CreateLobby("INGAME", isPublic: true)!;
        lobby.TryAddPlayer(new LobbyPlayer("c1", 1, "alice", false, true));
        lobby.TryAddPlayer(new LobbyPlayer("c2", 2, "bob",   false, false));
        lobby.StartGame(new Application.Lobbies.DTOs.GameSettingsDto(CardCount: 4, TurnTimeSeconds: 30));

        var result = manager.GetPublicLobbies();

        result.Should().NotContain(l => l.GameCode == "INGAME");
    }

    [Fact]
    public void GetPublicLobbies_EmptyManager_ReturnsEmpty()
    {
        var manager = new LobbyManager();

        var result = manager.GetPublicLobbies();

        result.Should().BeEmpty();
    }
}
