using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.Login;

public record LoginCommand(
    string Username,
    string Password
) : IRequest<AuthResponse>;
