using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.RefreshSession;

/// <summary>
/// Genera un nuevo access token usando un refresh token válido.
/// No rota el refresh token; la sesión se mantiene activa hasta su expiración natural.
/// </summary>
/// <param name="RefreshToken">Refresh token activo de la sesión.</param>
/// <param name="UserId">Identificador del usuario dueño del token.</param>
public record RefreshSessionCommand(
    string RefreshToken,
    int UserId
) : IRequest<AuthResponse>;
