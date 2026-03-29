using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Users;

public class UserSession : BaseEntity
{
    public string Token { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int UserId { get; private set; }

    private UserSession() { }

    public static UserSession Create(string token, int userId, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(DomainErrors.Session.TokenEmpty);

        return new UserSession
        {
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration)
        };
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

    public void Renew(TimeSpan duration)
    {
        ExpiresAt = DateTime.UtcNow.Add(duration);
    }
}
