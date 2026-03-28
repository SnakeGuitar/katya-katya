using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;

namespace MemoryGame.Domain.Penalties;

public class Penalty : BaseEntity
{
    public PenaltyType Type { get; private set; }
    public DateTime Duration { get; private set; }
    public int MatchId { get; private set; }
    public int UserId { get; private set; }

    private Penalty() { }

    public static Penalty Create(PenaltyType type, DateTime duration, int matchId, int userId)
    {
        return new Penalty
        {
            Type = type,
            Duration = duration,
            MatchId = matchId,
            UserId = userId
        };
    }

    public bool IsActive() => Type == PenaltyType.PermanentBan || DateTime.UtcNow < Duration;
}
