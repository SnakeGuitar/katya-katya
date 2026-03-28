namespace MemoryGame.Domain.Penalties;

public interface IPenaltyRepository
{
    Task<Penalty?> GetByIdAsync(int id);
    Task<IReadOnlyList<Penalty>> GetActiveByUserIdAsync(int userId);
    Task AddAsync(Penalty penalty);
}
