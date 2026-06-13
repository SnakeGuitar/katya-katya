using MediatR;
using KatyaKatya.Application.Auth.DTOs;

namespace KatyaKatya.Application.Auth.Queries.GetUserByUsernameQuery;

/// <summary>
/// Retrieves a user by their username.
/// </summary>
/// <param name="Username">The username to look up.</param>
public record GetUserByUsernameQuery(string Username) : IRequest<UserDto>;
