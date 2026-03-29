using MediatR;
using MemoryGame.Application.Social.DTOs;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Social.Queries.GetFriendsListQuery;

/// <summary>
/// Handles <see cref="GetFriendsListQuery"/>: retrieves the friend list of a user,
/// including each friend's username.
/// </summary>
public class GetFriendsListQueryHandler : IRequestHandler<GetFriendsListQuery, IReadOnlyList<FriendDto>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetFriendsListQueryHandler(ISocialRepository socialRepository, IUserRepository userRepository)
    {
        _socialRepository = socialRepository;
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FriendDto>> Handle(GetFriendsListQuery request, CancellationToken cancellationToken)
    {
        var friendships = await _socialRepository.GetFriendsAsync(request.UserId);

        var result = new List<FriendDto>(friendships.Count);

        foreach (var friendship in friendships)
        {
            var friendId = friendship.UserId == request.UserId ? friendship.FriendId : friendship.UserId;
            var friend = await _userRepository.GetByIdAsync(friendId);

            if (friend is not null)
                result.Add(new FriendDto(friend.Id, friend.Username, friendship.CreatedAt));
        }

        return result.AsReadOnly();
    }
}
