using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Domain.Users;

public interface IPendingRegistrationRepository
{
    Task<PendingRegistration?> GetByEmailAsync(Email email);
    Task AddAsync(PendingRegistration pendingRegistration);
    void Remove(PendingRegistration pendingRegistration);
}
