using MediatR;

namespace MemoryGame.Application.Profile.Commands.UpdatePersonalInfo;

/// <summary>
/// Actualiza el nombre y apellido del usuario.
/// Ambos campos son opcionales y pueden establecerse a <c>null</c> para borrarlos.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="Name">Nombre del usuario, o <c>null</c> para eliminar.</param>
/// <param name="LastName">Apellido del usuario, o <c>null</c> para eliminar.</param>
public record UpdatePersonalInfoCommand(
    int UserId,
    string? Name,
    string? LastName
) : IRequest<Unit>;
