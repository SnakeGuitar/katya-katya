using MediatR;
using MemoryGame.Application.Matches.DTOs;
using MemoryGame.Domain.Matches;

namespace MemoryGame.Application.Matches.Queries.GetMatchHistoryQuery;

/// <summary>
/// Handles <see cref="GetMatchHistoryQuery"/>: retrieves the match history for a user.
/// </summary>
public class GetMatchHistoryQueryHandler : IRequestHandler<GetMatchHistoryQuery, IReadOnlyList<MatchHistoryDto>>
{
    private readonly IMatchRepository _matchRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetMatchHistoryQueryHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MatchHistoryDto>> Handle(GetMatchHistoryQuery request, CancellationToken cancellationToken)
    {
        var participations = await _matchRepository.GetHistoryByUserIdAsync(request.UserId);

        return participations
            .Select(p => new MatchHistoryDto(
                MatchId: p.MatchId,
                Score: p.Score.Value,
                IsWinner: p.WinnerId == request.UserId))
            .ToList()
            .AsReadOnly();
    }
}
