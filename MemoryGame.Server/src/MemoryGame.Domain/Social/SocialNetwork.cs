using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Social;

public class SocialNetwork : BaseEntity
{
    public string? Account { get; private set; }
    public int UserId { get; private set; }

    private SocialNetwork() { }

    public static SocialNetwork Create(int userId, string account)
    {
        if (string.IsNullOrWhiteSpace(account))
            throw new DomainException("Social network account cannot be empty.");

        if (account.Length > 50)
            throw new DomainException("Social network account cannot exceed 50 characters.");

        return new SocialNetwork
        {
            UserId = userId,
            Account = account
        };
    }

    public void UpdateAccount(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
            throw new DomainException("Social network account cannot be empty.");

        Account = account;
    }
}
