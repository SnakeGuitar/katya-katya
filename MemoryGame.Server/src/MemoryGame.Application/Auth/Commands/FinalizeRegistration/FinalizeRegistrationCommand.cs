using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.FinalizeRegistration;

/// <summary>
/// Completa el registro del usuario validando el PIN y creando la cuenta.
/// Devuelve tokens de acceso y refresco listos para usar.
/// </summary>
/// <param name="Email">Email asociado al registro pendiente.</param>
/// <param name="Pin">PIN de 6 dígitos enviado al email.</param>
public record FinalizeRegistrationCommand(
    string Email,
    string Pin
) : IRequest<AuthResponse>;
