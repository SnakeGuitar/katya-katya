using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;

/// <summary>
/// Completa la conversión de una cuenta guest a cuenta registrada validando el PIN.
/// El usuario deja de ser guest y obtiene tokens actualizados.
/// </summary>
/// <param name="UserId">Identificador del usuario guest.</param>
/// <param name="Email">Email verificado con el PIN.</param>
/// <param name="Pin">PIN de 6 dígitos enviado al email.</param>
public record VerifyGuestUpgradeCommand(
    int UserId,
    string Email,
    string Pin
) : IRequest<AuthResponse>;
