using MediatR;
using KatyaKatya.Application.Social.DTOs;

namespace KatyaKatya.Application.Social.Queries.GetFriendsListQuery;

/// <summary>
/// Retrieves the friend list of a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public record GetFriendsListQuery(int UserId) : IRequest<IReadOnlyList<FriendDto>>;
