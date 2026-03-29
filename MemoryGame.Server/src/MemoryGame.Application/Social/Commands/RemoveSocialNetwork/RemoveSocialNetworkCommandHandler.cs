using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;

namespace MemoryGame.Application.Social.Commands.RemoveSocialNetwork;

/// <summary>
/// Handles <see cref="RemoveSocialNetworkCommand"/>: removes a social network entry
/// belonging to the requesting user.
/// </summary>
public class RemoveSocialNetworkCommandHandler : IRequestHandler<RemoveSocialNetworkCommand, Unit>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public RemoveSocialNetworkCommandHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(RemoveSocialNetworkCommand request, CancellationToken cancellationToken)
    {
        var networks = await _socialRepository.GetSocialNetworksAsync(request.UserId);
        var network = networks.FirstOrDefault(n => n.Id == request.SocialNetworkId)
            ?? throw new DomainException(DomainErrors.Social.SocialNetworkNotFound);

        _socialRepository.RemoveSocialNetwork(network);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
