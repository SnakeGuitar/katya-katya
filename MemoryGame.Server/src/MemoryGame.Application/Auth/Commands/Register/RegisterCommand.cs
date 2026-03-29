using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.Register;

/// <summary>
/// Inicia el proceso de registro de un nuevo usuario.
/// Genera un PIN de verificación y lo envía al email proporcionado.
/// El registro se completa con <see cref="FinalizeRegistration.FinalizeRegistrationCommand"/>.
/// </summary>
/// <param name="Username">Nombre de usuario único.</param>
/// <param name="Email">Dirección de correo electrónico del usuario.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<string>;
