using System.Collections.Concurrent;
using MemoryGame.Application.Lobbies.DTOs;

namespace MemoryGame.Application.Lobbies.Models;

/// <summary>
/// Represents an in-memory game lobby. Manages players, game state, and kick votes.
/// </summary>
public class Lobby
{
    public const int MaxPlayers = 4;

    public string GameCode { get; }
    public bool IsPublic { get; }
    public DateTime CreatedAt { get; }
    public GameSession? Game { get; private set; }

    private readonly ConcurrentDictionary<string, LobbyPlayer> _players = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _kickVotes = new();
    private readonly object _lock = new();

    public Lobby(string gameCode, bool isPublic)
    {
        GameCode = gameCode;
        IsPublic = isPublic;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// All players currently in the lobby, keyed by connection ID.
    /// </summary>
    public IReadOnlyDictionary<string, LobbyPlayer> Players => _players;

    /// <summary>
    /// Whether a game is currently in progress.
    /// </summary>
    public bool IsGameInProgress => Game is not null && !Game.IsFinished;

    /// <summary>
    /// Attempts to add a player to the lobby. Returns false if the lobby is full.
    /// </summary>
    public bool TryAddPlayer(LobbyPlayer player)
    {
        lock (_lock)
        {
            if (_players.Count >= MaxPlayers)
                return false;

            return _players.TryAdd(player.ConnectionId, player);
        }
    }

    /// <summary>
    /// Removes a player from the lobby. Returns the removed player, or null if not found.
    /// Promotes the next player to host if the host leaves.
    /// </summary>
    public LobbyPlayer? RemovePlayer(string connectionId)
    {
        if (!_players.TryRemove(connectionId, out var player))
            return null;

        Game?.RemovePlayer(player.Username);
        _kickVotes.TryRemove(player.Username, out _);

        if (player.IsHost && !_players.IsEmpty)
        {
            var newHost = _players.Values.OrderBy(p => p.JoinedAt).First();
            newHost.IsHost = true;
        }

        return player;
    }

    /// <summary>
    /// Gets a player by their connection ID.
    /// </summary>
    public LobbyPlayer? GetPlayer(string connectionId)
    {
        _players.TryGetValue(connectionId, out var player);
        return player;
    }

    /// <summary>
    /// Gets the lobby host, or null if the lobby is empty.
    /// </summary>
    public LobbyPlayer? GetHost() => _players.Values.FirstOrDefault(p => p.IsHost);

    /// <summary>
    /// Starts a new game session with the given settings.
    /// </summary>
    public GameSession StartGame(GameSettingsDto settings)
    {
        var usernames = _players.Values.OrderBy(p => p.JoinedAt).Select(p => p.Username);
        Game = new GameSession(settings.CardCount, settings.TurnTimeSeconds, usernames);
        return Game;
    }

    /// <summary>
    /// Registers a vote to kick a player. Returns true if the threshold is reached (majority).
    /// </summary>
    public bool VoteToKick(string voterUsername, string targetUsername)
    {
        var votes = _kickVotes.GetOrAdd(targetUsername, _ => new HashSet<string>());

        lock (_lock)
        {
            votes.Add(voterUsername);
            var threshold = (_players.Count / 2) + 1;
            return votes.Count >= threshold;
        }
    }

    /// <summary>
    /// Builds a summary DTO for public lobby listing.
    /// </summary>
    public LobbySummaryDto ToSummary()
    {
        var host = GetHost();
        return new LobbySummaryDto(
            GameCode,
            host?.Username ?? "Unknown",
            _players.Count,
            MaxPlayers,
            _players.Count >= MaxPlayers);
    }

    /// <summary>
    /// Gets all current players as DTOs.
    /// </summary>
    public LobbyPlayerDto[] GetPlayerList()
    {
        return _players.Values
            .OrderBy(p => p.JoinedAt)
            .Select(p => new LobbyPlayerDto(p.UserId, p.Username, p.IsGuest, p.IsHost))
            .ToArray();
    }
}
