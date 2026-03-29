using MediatR;

namespace MemoryGame.Application.Profile.Commands.ChangeUsername;

/// <summary>
/// Cambia el nombre de usuario validando que el nuevo username no esté en uso.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="NewUsername">Nuevo nombre de usuario único.</param>
public record ChangeUsernameCommand(
    int UserId,
    string NewUsername
) : IRequest<Unit>;
