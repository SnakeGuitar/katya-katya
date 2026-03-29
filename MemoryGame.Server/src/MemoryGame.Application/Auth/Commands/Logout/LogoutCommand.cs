using MediatR;

namespace MemoryGame.Application.Auth.Commands.Logout;

/// <summary>
/// Cierra la sesión del usuario revocando todos sus refresh tokens activos.
/// </summary>
/// <param name="UserId">Identificador del usuario que cierra sesión.</param>
public record LogoutCommand(
    int UserId
) : IRequest<Unit>;
