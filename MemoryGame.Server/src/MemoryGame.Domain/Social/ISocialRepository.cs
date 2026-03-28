namespace MemoryGame.Domain.Social;

public interface ISocialRepository
{
    Task<FriendRequest?> GetFriendRequestByIdAsync(int requestId);
    Task<IReadOnlyList<FriendRequest>> GetPendingRequestsAsync(int userId);
    Task<FriendRequest?> GetPendingRequestBetweenAsync(int senderId, int receiverId);
    Task AddFriendRequestAsync(FriendRequest request);

    Task<IReadOnlyList<Friendship>> GetFriendsAsync(int userId);
    Task<bool> AreFriendsAsync(int userId, int friendId);
    Task AddFriendshipAsync(Friendship friendship);
    void RemoveFriendship(Friendship friendship);

    Task<IReadOnlyList<SocialNetwork>> GetSocialNetworksAsync(int userId);
    Task AddSocialNetworkAsync(SocialNetwork socialNetwork);
    void RemoveSocialNetwork(SocialNetwork socialNetwork);
}
