using MediatR;

namespace MemoryGame.Application.Profile.Commands.GetAvatar;

/// <summary>
/// Obtiene los bytes del avatar del usuario.
/// Devuelve <c>null</c> si el usuario no tiene avatar configurado.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
public record GetAvatarCommand(
    int UserId
) : IRequest<byte[]?>;
