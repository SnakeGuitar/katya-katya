using MediatR;

namespace MemoryGame.Application.Social.Commands.RemoveSocialNetwork;

/// <summary>
/// Removes a social network account from a user's profile.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="SocialNetworkId">The identifier of the social network entry to remove.</param>
public record RemoveSocialNetworkCommand(int UserId, int SocialNetworkId) : IRequest<Unit>;
