using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;

namespace MemoryGame.Domain.Social;

public class FriendRequest : BaseEntity
{
    public int SenderId { get; private set; }
    public int ReceiverId { get; private set; }
    public FriendRequestStatus Status { get; private set; }
    public DateTime SentAt { get; private set; }

    private FriendRequest() { }

    public static FriendRequest Create(int senderId, int receiverId)
    {
        if (senderId == receiverId)
            throw new DomainException("Cannot send a friend request to yourself.");

        return new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            SentAt = DateTime.UtcNow
        };
    }

    public void Accept()
    {
        if (Status != FriendRequestStatus.Pending)
            throw new DomainException("Only pending requests can be accepted.");

        Status = FriendRequestStatus.Accepted;
    }

    public void Reject()
    {
        if (Status != FriendRequestStatus.Pending)
            throw new DomainException("Only pending requests can be rejected.");

        Status = FriendRequestStatus.Rejected;
    }
}
