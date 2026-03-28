using MemoryGame.Application.Common.Interfaces;

namespace MemoryGame.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MemoryGameDbContext _context;

    public UnitOfWork(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
