namespace MemoryGame.Application.Lobbies.DTOs;

/// <summary>
/// Represents a player currently connected to a lobby.
/// </summary>
/// <param name="UserId">The player's user identifier.</param>
/// <param name="Username">The player's display name.</param>
/// <param name="IsGuest">Whether the player is a guest account.</param>
/// <param name="IsHost">Whether the player is the lobby host.</param>
public record LobbyPlayerDto(int UserId, string Username, bool IsGuest, bool IsHost);
