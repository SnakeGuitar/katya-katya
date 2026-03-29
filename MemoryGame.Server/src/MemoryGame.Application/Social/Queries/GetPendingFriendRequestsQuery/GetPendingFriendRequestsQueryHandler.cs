using MediatR;
using MemoryGame.Application.Social.DTOs;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Social.Queries.GetPendingFriendRequestsQuery;

/// <summary>
/// Handles <see cref="GetPendingFriendRequestsQuery"/>: retrieves all pending friend requests
/// received by a user, including the sender's username.
/// </summary>
public class GetPendingFriendRequestsQueryHandler : IRequestHandler<GetPendingFriendRequestsQuery, IReadOnlyList<FriendRequestDto>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetPendingFriendRequestsQueryHandler(ISocialRepository socialRepository, IUserRepository userRepository)
    {
        _socialRepository = socialRepository;
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FriendRequestDto>> Handle(GetPendingFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _socialRepository.GetPendingRequestsAsync(request.UserId);

        var result = new List<FriendRequestDto>(requests.Count);

        foreach (var friendRequest in requests)
        {
            var sender = await _userRepository.GetByIdAsync(friendRequest.SenderId);

            if (sender is not null)
                result.Add(new FriendRequestDto(friendRequest.Id, sender.Id, sender.Username, friendRequest.SentAt));
        }

        return result.AsReadOnly();
    }
}
