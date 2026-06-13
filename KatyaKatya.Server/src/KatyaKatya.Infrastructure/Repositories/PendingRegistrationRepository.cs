using Microsoft.EntityFrameworkCore;
using KatyaKatya.Domain.Users;
using KatyaKatya.Domain.Users.ValueObjects;
using KatyaKatya.Infrastructure.Persistence;

namespace KatyaKatya.Infrastructure.Repositories;

public class PendingRegistrationRepository : IPendingRegistrationRepository
{
    private readonly MemoryGameDbContext _context;

    public PendingRegistrationRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<PendingRegistration?> GetByEmailAsync(Email email)
    {
        return await _context.PendingRegistrations
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task AddAsync(PendingRegistration pendingRegistration)
    {
        await _context.PendingRegistrations.AddAsync(pendingRegistration);
    }

    public void Remove(PendingRegistration pendingRegistration)
    {
        _context.PendingRegistrations.Remove(pendingRegistration);
    }
}
