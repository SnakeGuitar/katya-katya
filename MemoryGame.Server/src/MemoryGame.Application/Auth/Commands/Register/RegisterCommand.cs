using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<string>;
