namespace MemoryGame.Domain.Matches;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int id);
    Task<IReadOnlyList<MatchParticipation>> GetHistoryByUserIdAsync(int userId);
    Task AddAsync(Match match);
    void Update(Match match);
}
