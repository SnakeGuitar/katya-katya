namespace MemoryGame.Client.Models.Lobby;

/// <summary>
/// Game configuration set by the lobby host before starting a match.
/// </summary>
/// <param name="CardCount">Total number of cards on the board (must be even).</param>
/// <param name="TurnTimeSeconds">Seconds allowed per turn before automatic skip.</param>
public record GameSettingsDto(int CardCount, int TurnTimeSeconds);
