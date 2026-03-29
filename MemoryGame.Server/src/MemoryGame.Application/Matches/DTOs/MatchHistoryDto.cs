namespace MemoryGame.Application.Matches.DTOs;

/// <summary>
/// Represents a match entry in the user's match history.
/// </summary>
/// <param name="MatchId">The match identifier.</param>
/// <param name="Score">The score the user achieved in the match.</param>
/// <param name="IsWinner">Whether the user won the match.</param>
public record MatchHistoryDto(int MatchId, int Score, bool IsWinner);
