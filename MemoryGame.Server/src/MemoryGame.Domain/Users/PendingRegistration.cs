using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Domain.Users;

public class PendingRegistration : BaseEntity
{
    public Email Email { get; private set; } = null!;
    public string Pin { get; private set; } = null!;
    public string? HashedPassword { get; private set; }
    public DateTime ExpirationTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PendingRegistration() { }

    public static PendingRegistration Create(string email, string pin, string hashedPassword, TimeSpan validity)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length > 10)
            throw new DomainException(DomainErrors.PendingRegistration.PinInvalidFormat);

        return new PendingRegistration
        {
            Email = Email.Create(email),
            Pin = pin,
            HashedPassword = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow.Add(validity)
        };
    }

    public static PendingRegistration CreateForUpgrade(Email email, string pin, string hashedPassword, int userId)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length > 10)
            throw new DomainException(DomainErrors.PendingRegistration.PinInvalidFormat);

        return new PendingRegistration
        {
            Email = email,
            Pin = pin,
            HashedPassword = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(15))
        };
    }

    public void UpdatePin(string newPin)
    {
        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length > 10)
            throw new DomainException(DomainErrors.PendingRegistration.PinInvalidFormat);

        Pin = newPin;
        ExpirationTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(15));
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpirationTime;

    public bool ValidatePin(string pin) => Pin == pin && !IsExpired();
}
