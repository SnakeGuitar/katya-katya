using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;

namespace MemoryGame.Domain.Matches;

public class Match : BaseEntity
{
    public DateTime StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public MatchStatus Status { get; private set; }

    private readonly List<MatchParticipation> _participations = [];
    public IReadOnlyCollection<MatchParticipation> Participations => _participations.AsReadOnly();

    private Match() { }

    public static Match Create()
    {
        return new Match
        {
            StartDateTime = DateTime.UtcNow,
            Status = MatchStatus.InProgress
        };
    }

    public MatchParticipation AddParticipant(int userId)
    {
        if (Status != MatchStatus.InProgress)
            throw new DomainException("Cannot add participants to a finished match.");

        if (_participations.Any(p => p.UserId == userId))
            throw new DomainException("User is already a participant.");

        var participation = MatchParticipation.Create(Id, userId);
        _participations.Add(participation);
        return participation;
    }

    public void Finish(int? winnerId)
    {
        if (Status != MatchStatus.InProgress)
            throw new DomainException("Match is not in progress.");

        if (winnerId.HasValue && _participations.All(p => p.UserId != winnerId.Value))
            throw new DomainException("Winner must be a participant.");

        foreach (var participation in _participations)
        {
            participation.SetWinner(winnerId);
        }

        EndDateTime = DateTime.UtcNow;
        Status = MatchStatus.Finished;
    }

    public void Cancel()
    {
        if (Status != MatchStatus.InProgress)
            throw new DomainException("Only in-progress matches can be cancelled.");

        EndDateTime = DateTime.UtcNow;
        Status = MatchStatus.Cancelled;
    }
}
