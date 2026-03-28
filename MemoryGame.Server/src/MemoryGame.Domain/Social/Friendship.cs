using MemoryGame.Domain.Common;

namespace MemoryGame.Domain.Social;

public class Friendship : BaseEntity
{
    public int UserId { get; private set; }
    public int FriendId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Friendship() { }

    public static Friendship Create(int userId, int friendId)
    {
        if (userId == friendId)
            throw new DomainException("Cannot be friends with yourself.");

        return new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
