using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.GetAvatar;

/// <summary>
/// Maneja <see cref="GetAvatarCommand"/>: recupera los bytes del avatar del usuario.
/// </summary>
public class GetAvatarCommandHandler : IRequestHandler<GetAvatarCommand, byte[]?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Inicializa el handler con sus dependencias.
    /// </summary>
    public GetAvatarCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<byte[]?> Handle(GetAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        return user.Avatar;
    }
}
