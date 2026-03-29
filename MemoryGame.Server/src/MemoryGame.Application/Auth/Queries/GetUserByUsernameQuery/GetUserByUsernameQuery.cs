using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Queries.GetUserByUsernameQuery;

/// <summary>
/// Retrieves a user by their username.
/// </summary>
/// <param name="Username">The username to look up.</param>
public record GetUserByUsernameQuery(string Username) : IRequest<UserDto>;
