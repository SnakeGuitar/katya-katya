namespace MemoryGame.Client.Models;

/// <summary>
/// DTO for a pending friend request.
/// </summary>
public record FriendRequestDto(int RequestId, int SenderId, string SenderUsername, DateTime SentAt);
