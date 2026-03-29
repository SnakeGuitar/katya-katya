using MediatR;

namespace MemoryGame.Application.Profile.Commands.UpdateAvatar;

/// <summary>
/// Actualiza el avatar del usuario con los bytes de la nueva imagen.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="AvatarData">Bytes de la imagen del nuevo avatar.</param>
public record UpdateAvatarCommand(
    int UserId,
    byte[] AvatarData
) : IRequest<Unit>;
