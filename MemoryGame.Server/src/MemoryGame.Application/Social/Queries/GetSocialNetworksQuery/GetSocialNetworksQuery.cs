using MediatR;
using MemoryGame.Application.Social.DTOs;

namespace MemoryGame.Application.Social.Queries.GetSocialNetworksQuery;

/// <summary>
/// Retrieves all social network accounts linked to a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public record GetSocialNetworksQuery(int UserId) : IRequest<IReadOnlyList<SocialNetworkDto>>;
