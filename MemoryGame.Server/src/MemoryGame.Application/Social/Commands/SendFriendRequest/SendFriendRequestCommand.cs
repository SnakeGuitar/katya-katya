using MediatR;

namespace MemoryGame.Application.Social.Commands.SendFriendRequest;

/// <summary>
/// Sends a friend request from one user to another.
/// </summary>
/// <param name="SenderId">The identifier of the user sending the request.</param>
/// <param name="ReceiverUsername">The username of the user receiving the request.</param>
public record SendFriendRequestCommand(int SenderId, string ReceiverUsername) : IRequest<Unit>;
