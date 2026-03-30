namespace MemoryGame.Application.Lobbies.Interfaces;

/// <summary>
/// Tracks which authenticated users are currently connected to the hub
/// and maps them to their active SignalR connection ID.
/// </summary>
public interface IPresenceTracker
{
    /// <summary>
    /// Registers a user as online with the given connection ID.
    /// </summary>
    void Track(int userId, string connectionId);

    /// <summary>
    /// Removes the mapping for the given connection ID.
    /// </summary>
    void Untrack(string connectionId);

    /// <summary>
    /// Returns the active connection ID for a user, or null if they are offline.
    /// </summary>
    string? GetConnectionId(int userId);

    /// <summary>
    /// Returns true if the user currently has an active hub connection.
    /// </summary>
    bool IsOnline(int userId);
}
