using MediatR;
using MemoryGame.Application.Matches.DTOs;

namespace MemoryGame.Application.Matches.Queries.GetMatchHistoryQuery;

/// <summary>
/// Retrieves the match history for a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public record GetMatchHistoryQuery(int UserId) : IRequest<IReadOnlyList<MatchHistoryDto>>;
