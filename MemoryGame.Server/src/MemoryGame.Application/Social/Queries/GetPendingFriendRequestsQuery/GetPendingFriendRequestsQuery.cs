using MediatR;
using MemoryGame.Application.Social.DTOs;

namespace MemoryGame.Application.Social.Queries.GetPendingFriendRequestsQuery;

/// <summary>
/// Retrieves all pending friend requests received by a user.
/// </summary>
/// <param name="UserId">The identifier of the user receiving the requests.</param>
public record GetPendingFriendRequestsQuery(int UserId) : IRequest<IReadOnlyList<FriendRequestDto>>;
