namespace MemoryGame.Client.Models.Lobby;

/// <summary>
/// Represents a card on the game board as seen by clients.
/// The <see cref="ImageIdentifier"/> is only sent when the card is face-up.
/// </summary>
/// <param name="Index">The card's position on the board.</param>
/// <param name="ImageIdentifier">The image identifier, or null if the card is face-down.</param>
/// <param name="IsMatched">Whether this card has already been matched.</param>
public record CardInfoDto(int Index, string? ImageIdentifier, bool IsMatched);
