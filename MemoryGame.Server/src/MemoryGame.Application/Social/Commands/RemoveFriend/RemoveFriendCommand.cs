using MediatR;

namespace MemoryGame.Application.Social.Commands.RemoveFriend;

/// <summary>
/// Removes a friendship between two users.
/// </summary>
/// <param name="UserId">The identifier of the user initiating the removal.</param>
/// <param name="FriendId">The identifier of the friend to remove.</param>
public record RemoveFriendCommand(int UserId, int FriendId) : IRequest<Unit>;
