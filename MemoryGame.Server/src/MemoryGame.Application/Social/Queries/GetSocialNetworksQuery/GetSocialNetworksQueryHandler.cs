using MediatR;
using MemoryGame.Application.Social.DTOs;
using MemoryGame.Domain.Social;

namespace MemoryGame.Application.Social.Queries.GetSocialNetworksQuery;

/// <summary>
/// Handles <see cref="GetSocialNetworksQuery"/>: retrieves all social network accounts linked to a user.
/// </summary>
public class GetSocialNetworksQueryHandler : IRequestHandler<GetSocialNetworksQuery, IReadOnlyList<SocialNetworkDto>>
{
    private readonly ISocialRepository _socialRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetSocialNetworksQueryHandler(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SocialNetworkDto>> Handle(GetSocialNetworksQuery request, CancellationToken cancellationToken)
    {
        var networks = await _socialRepository.GetSocialNetworksAsync(request.UserId);

        return networks
            .Select(n => new SocialNetworkDto(n.Id, n.Account!))
            .ToList()
            .AsReadOnly();
    }
}
