using MemoryGame.Domain.Common;
using MemoryGame.Domain.Matches.ValueObjects;

namespace MemoryGame.Domain.Matches;

public class MatchParticipation
{
    public int UserId { get; private set; }
    public int MatchId { get; private set; }
    public Score Score { get; private set; } = Score.Zero;
    public int? WinnerId { get; private set; }

    private MatchParticipation() { }

    internal static MatchParticipation Create(int matchId, int userId)
    {
        return new MatchParticipation
        {
            MatchId = matchId,
            UserId = userId,
            Score = Score.Zero
        };
    }

    public void AddPoints(int points)
    {
        Score = Score.Add(points);
    }

    internal void SetWinner(int? winnerId)
    {
        WinnerId = winnerId;
    }
}
