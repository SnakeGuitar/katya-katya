using MediatR;

namespace MemoryGame.Application.Social.Commands.AnswerFriendRequest;

/// <summary>
/// Accepts or rejects a pending friend request.
/// </summary>
/// <param name="UserId">The identifier of the user answering the request (must be the receiver).</param>
/// <param name="RequestId">The identifier of the friend request.</param>
/// <param name="Accept">True to accept, false to reject.</param>
public record AnswerFriendRequestCommand(int UserId, int RequestId, bool Accept) : IRequest<Unit>;
