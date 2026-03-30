namespace MemoryGame.Application.Lobbies.Models;

/// <summary>
/// Represents a player connected to a lobby, holding their connection and identity info.
/// </summary>
public class LobbyPlayer
{
    public string ConnectionId { get; }
    public int UserId { get; }
    public string Username { get; }
    public bool IsGuest { get; }
    public bool IsHost { get; set; }
    public DateTime JoinedAt { get; }

    public LobbyPlayer(string connectionId, int userId, string username, bool isGuest, bool isHost)
    {
        ConnectionId = connectionId;
        UserId = userId;
        Username = username;
        IsGuest = isGuest;
        IsHost = isHost;
        JoinedAt = DateTime.UtcNow;
    }
}
