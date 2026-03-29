namespace MemoryGame.Application.Social.DTOs;

/// <summary>
/// Represents a pending friend request received by the user.
/// </summary>
/// <param name="RequestId">The friend request identifier.</param>
/// <param name="SenderId">The identifier of the user who sent the request.</param>
/// <param name="SenderUsername">The username of the sender.</param>
/// <param name="SentAt">The date and time the request was sent.</param>
public record FriendRequestDto(int RequestId, int SenderId, string SenderUsername, DateTime SentAt);
