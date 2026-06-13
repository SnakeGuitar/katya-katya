using KatyaKatya.Domain.Users.ValueObjects;

namespace KatyaKatya.Domain.Users;

public interface IPendingRegistrationRepository
{
    Task<PendingRegistration?> GetByEmailAsync(Email email);
    Task AddAsync(PendingRegistration pendingRegistration);
    void Remove(PendingRegistration pendingRegistration);
}
