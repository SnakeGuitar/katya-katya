using MediatR;
using KatyaKatya.Application.Common.Interfaces;
using KatyaKatya.Application.Social.DTOs;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Social;
using KatyaKatya.Domain.Users;

namespace KatyaKatya.Application.Social.Commands.AddSocialNetwork;

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
