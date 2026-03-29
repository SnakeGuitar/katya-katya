namespace MemoryGame.Application.Social.DTOs;

/// <summary>
/// Represents a friend in the user's friend list.
/// </summary>
/// <param name="UserId">The friend's user identifier.</param>
/// <param name="Username">The friend's username.</param>
/// <param name="FriendsSince">The date the friendship was established.</param>
public record FriendDto(int UserId, string Username, DateTime FriendsSince);
