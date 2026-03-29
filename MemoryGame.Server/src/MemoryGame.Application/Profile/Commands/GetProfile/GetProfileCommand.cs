using MediatR;
using MemoryGame.Application.Profile.DTOs;

namespace MemoryGame.Application.Profile.Commands.GetProfile;

/// <summary>
/// Obtiene el perfil completo del usuario incluyendo datos personales y avatar.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
public record GetProfileCommand(
    int UserId
) : IRequest<ProfileResponse>;
