using MemoryGame.Application.Lobbies.DTOs;
using MemoryGame.Application.Lobbies.Models;

namespace MemoryGame.Application.Lobbies.Interfaces;

/// <summary>
/// Manages the lifecycle of in-memory game lobbies.
/// </summary>
public interface ILobbyManager
{
    /// <summary>
    /// Creates a new lobby with the given code. Returns the lobby, or null if the code is taken.
    /// </summary>
    Lobby? CreateLobby(string gameCode, bool isPublic);

    /// <summary>
    /// Retrieves a lobby by its game code.
    /// </summary>
    Lobby? GetLobby(string gameCode);

    /// <summary>
    /// Removes a lobby by its game code. Used when all players leave.
    /// </summary>
    bool RemoveLobby(string gameCode);

    /// <summary>
    /// Returns summaries of all public, non-full lobbies.
    /// </summary>
    IReadOnlyList<LobbySummaryDto> GetPublicLobbies();

    /// <summary>
    /// Finds which lobby a connection ID is currently in.
    /// </summary>
    Lobby? FindLobbyByConnection(string connectionId);

    /// <summary>
    /// Finds which lobby a user ID is currently in.
    /// </summary>
    Lobby? FindLobbyByUserId(int userId);
}
