using System.Collections.Concurrent;
using MemoryGame.Application.Lobbies.Interfaces;

namespace MemoryGame.Infrastructure.Lobbies;

/// <summary>
/// In-memory presence registry. Maintains two dictionaries for O(1) lookup
/// in both directions: userId → connectionId and connectionId → userId.
/// Registered as a singleton.
/// </summary>
public sealed class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<int, string> _userToConnection = new();
    private readonly ConcurrentDictionary<string, int> _connectionToUser = new();

    /// <inheritdoc/>
    public void Track(int userId, string connectionId)
    {
        _userToConnection[userId] = connectionId;
        _connectionToUser[connectionId] = userId;
    }

    /// <inheritdoc/>
    public void Untrack(string connectionId)
    {
        if (_connectionToUser.TryRemove(connectionId, out var userId))
            _userToConnection.TryRemove(userId, out _);
    }

    /// <inheritdoc/>
    public string? GetConnectionId(int userId)
    {
        _userToConnection.TryGetValue(userId, out var connectionId);
        return connectionId;
    }

    /// <inheritdoc/>
    public bool IsOnline(int userId) => _userToConnection.ContainsKey(userId);
}
