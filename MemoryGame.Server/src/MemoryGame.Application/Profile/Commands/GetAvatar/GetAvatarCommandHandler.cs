using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.GetAvatar;

/// <summary>
/// Handles <see cref="GetAvatarCommand"/>: retrieves the avatar bytes of the user.
/// </summary>
public class GetAvatarCommandHandler : IRequestHandler<GetAvatarCommand, byte[]?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetAvatarCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<byte[]?> Handle(GetAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        return user.Avatar;
    }
}
