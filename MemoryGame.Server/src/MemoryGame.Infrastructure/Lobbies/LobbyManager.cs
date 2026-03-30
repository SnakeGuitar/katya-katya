using System.Collections.Concurrent;
using MemoryGame.Application.Lobbies.DTOs;
using MemoryGame.Application.Lobbies.Interfaces;
using MemoryGame.Application.Lobbies.Models;

namespace MemoryGame.Infrastructure.Lobbies;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ILobbyManager"/>.
/// Lobbies are transient and exist only while players are connected.
/// </summary>
public class LobbyManager : ILobbyManager
{
    private readonly ConcurrentDictionary<string, Lobby> _lobbies = new();

    /// <inheritdoc/>
    public Lobby? CreateLobby(string gameCode, bool isPublic)
    {
        var lobby = new Lobby(gameCode, isPublic);
        return _lobbies.TryAdd(gameCode, lobby) ? lobby : null;
    }

    /// <inheritdoc/>
    public Lobby? GetLobby(string gameCode)
    {
        _lobbies.TryGetValue(gameCode, out var lobby);
        return lobby;
    }

    /// <inheritdoc/>
    public bool RemoveLobby(string gameCode)
    {
        return _lobbies.TryRemove(gameCode, out _);
    }

    /// <inheritdoc/>
    public IReadOnlyList<LobbySummaryDto> GetPublicLobbies()
    {
        return _lobbies.Values
            .Where(l => l.IsPublic && !l.IsGameInProgress)
            .Select(l => l.ToSummary())
            .Where(s => !s.IsFull)
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public Lobby? FindLobbyByConnection(string connectionId)
    {
        return _lobbies.Values.FirstOrDefault(l => l.GetPlayer(connectionId) is not null);
    }
}
