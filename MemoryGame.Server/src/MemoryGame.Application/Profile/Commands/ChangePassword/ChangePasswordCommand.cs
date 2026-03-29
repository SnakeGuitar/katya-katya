using MediatR;

namespace MemoryGame.Application.Profile.Commands.ChangePassword;

/// <summary>
/// Cambia la contraseña del usuario verificando primero la contraseña actual.
/// No disponible para cuentas guest.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="CurrentPassword">Contraseña actual en texto plano.</param>
/// <param name="NewPassword">Nueva contraseña en texto plano.</param>
public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Unit>;
