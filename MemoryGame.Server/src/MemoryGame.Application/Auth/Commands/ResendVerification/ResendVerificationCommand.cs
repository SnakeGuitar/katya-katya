using MediatR;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Reenvía el PIN de verificación al email del registro pendiente.
/// Genera un nuevo PIN e invalida el anterior.
/// </summary>
/// <param name="Email">Email asociado al registro pendiente.</param>
public record ResendVerificationCommand(
    string Email
) : IRequest<string>;
