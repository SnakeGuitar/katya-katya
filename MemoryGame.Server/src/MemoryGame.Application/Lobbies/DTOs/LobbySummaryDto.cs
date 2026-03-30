namespace MemoryGame.Application.Lobbies.DTOs;

/// <summary>
/// Summary of a public lobby for lobby listing.
/// </summary>
/// <param name="GameCode">The unique lobby code.</param>
/// <param name="HostUsername">The username of the lobby host.</param>
/// <param name="CurrentPlayers">Number of players currently in the lobby.</param>
/// <param name="MaxPlayers">Maximum number of players allowed.</param>
/// <param name="IsFull">Whether the lobby has reached its player limit.</param>
public record LobbySummaryDto(string GameCode, string HostUsername, int CurrentPlayers, int MaxPlayers, bool IsFull);
