namespace MemoryGame.Client.Models;

/// <summary>
/// DTO for a friend in the user's friend list.
/// </summary>
public record FriendDto(int UserId, string Username, DateTime FriendsSince);
