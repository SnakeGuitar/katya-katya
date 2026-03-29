using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.LoginAsGuest;

/// <summary>
/// Crea y autentica una cuenta de invitado sin necesidad de email ni contraseña.
/// Las cuentas guest pueden convertirse a registradas con <see cref="UpgradeGuest.UpgradeGuestCommand"/>.
/// </summary>
/// <param name="GuestUsername">Nombre de usuario temporal para el invitado.</param>
public record LoginAsGuestCommand(
    string GuestUsername
) : IRequest<AuthResponse>;
