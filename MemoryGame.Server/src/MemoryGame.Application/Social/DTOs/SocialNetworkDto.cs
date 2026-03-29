namespace MemoryGame.Application.Social.DTOs;

/// <summary>
/// Represents a social network account linked to a user.
/// </summary>
/// <param name="Id">The social network entry identifier.</param>
/// <param name="Account">The social network account name or handle.</param>
public record SocialNetworkDto(int Id, string Account);
