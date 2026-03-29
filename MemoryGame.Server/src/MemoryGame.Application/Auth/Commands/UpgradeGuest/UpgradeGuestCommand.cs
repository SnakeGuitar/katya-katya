using MediatR;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

/// <summary>
/// Inicia el proceso de conversión de una cuenta guest a cuenta registrada.
/// Envía un PIN de verificación al email proporcionado.
/// El proceso se completa con <see cref="VerifyGuestUpgrade.VerifyGuestUpgradeCommand"/>.
/// </summary>
/// <param name="UserId">Identificador del usuario guest.</param>
/// <param name="Email">Email que se asociará a la cuenta.</param>
/// <param name="Password">Contraseña en texto plano para la nueva cuenta.</param>
public record UpgradeGuestCommand(
    int UserId,
    string Email,
    string Password
) : IRequest<string>;
