using MediatR;
using KatyaKatya.Application.Auth.DTOs;

namespace KatyaKatya.Application.Auth.Commands.Login;

/// <summary>
/// Autentica a un usuario registrado con sus credenciales.
/// Devuelve tokens de acceso y refresco en caso de éxito.
/// </summary>
/// <param name="Username">Nombre de usuario.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public record LoginCommand(
    string Username,
    string Password
) : IRequest<AuthResponse>;
