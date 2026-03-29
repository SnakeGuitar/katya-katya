using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Application.Social.DTOs;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Social.Commands.AddSocialNetwork;

/// <summary>
/// Handles <see cref="AddSocialNetworkCommand"/>: creates a new social network entry
/// for the user and persists it.
/// </summary>
public class AddSocialNetworkCommandHandler : IRequestHandler<AddSocialNetworkCommand, SocialNetworkDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public AddSocialNetworkCommandHandler(
        IUserRepository userRepository,
        ISocialRepository socialRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<SocialNetworkDto> Handle(AddSocialNetworkCommand request, CancellationToken cancellationToken)
    {
        _ = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        var socialNetwork = SocialNetwork.Create(request.UserId, request.Account);

        await _socialRepository.AddSocialNetworkAsync(socialNetwork);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SocialNetworkDto(socialNetwork.Id, socialNetwork.Account!);
    }
}
