using MediatR;

namespace MemoryGame.Application.Auth.Commands.VerifyRegistration;

/// <summary>
/// Verifica que el PIN de registro sea válido sin crear el usuario todavía.
/// Útil para validar el PIN antes de mostrar el formulario de datos adicionales.
/// </summary>
/// <param name="Email">Email asociado al registro pendiente.</param>
/// <param name="Pin">PIN de 6 dígitos enviado al email.</param>
public record VerifyRegistrationCommand(
    string Email,
    string Pin
) : IRequest<bool>;
