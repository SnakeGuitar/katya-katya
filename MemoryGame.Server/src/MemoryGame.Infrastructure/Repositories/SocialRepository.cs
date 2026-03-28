using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Social;
using MemoryGame.Infrastructure.Persistence;

namespace MemoryGame.Infrastructure.Repositories;

public class SocialRepository : ISocialRepository
{
    private readonly MemoryGameDbContext _context;

    public SocialRepository(MemoryGameDbContext context)
    {
        _context = context;
    }

    public async Task<FriendRequest?> GetFriendRequestByIdAsync(int requestId)
    {
        return await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<IReadOnlyList<FriendRequest>> GetPendingRequestsAsync(int userId)
    {
        return await _context.FriendRequests
            .Where(r => r.ReceiverId == userId && r.Status == Domain.Common.Enums.FriendRequestStatus.Pending)
            .ToListAsync();
    }

    public async Task<FriendRequest?> GetPendingRequestBetweenAsync(int senderId, int receiverId)
    {
        return await _context.FriendRequests
            .FirstOrDefaultAsync(r =>
                r.SenderId == senderId &&
                r.ReceiverId == receiverId &&
                r.Status == Domain.Common.Enums.FriendRequestStatus.Pending);
    }

    public async Task AddFriendRequestAsync(FriendRequest request)
    {
        await _context.FriendRequests.AddAsync(request);
    }

    public async Task<IReadOnlyList<Friendship>> GetFriendsAsync(int userId)
    {
        return await _context.Friendships
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> AreFriendsAsync(int userId, int friendId)
    {
        return await _context.Friendships
            .AnyAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
                          (f.UserId == friendId && f.FriendId == userId));
    }

    public async Task AddFriendshipAsync(Friendship friendship)
    {
        await _context.Friendships.AddAsync(friendship);
    }

    public void RemoveFriendship(Friendship friendship)
    {
        _context.Friendships.Remove(friendship);
    }

    public async Task<IReadOnlyList<SocialNetwork>> GetSocialNetworksAsync(int userId)
    {
        return await _context.SocialNetworks
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task AddSocialNetworkAsync(SocialNetwork socialNetwork)
    {
        await _context.SocialNetworks.AddAsync(socialNetwork);
    }

    public void RemoveSocialNetwork(SocialNetwork socialNetwork)
    {
        _context.SocialNetworks.Remove(socialNetwork);
    }
}
