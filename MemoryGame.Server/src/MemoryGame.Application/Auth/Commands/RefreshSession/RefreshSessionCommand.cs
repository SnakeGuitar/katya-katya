using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(
    string RefreshToken,
    int UserId
) : IRequest<AuthResponse>;
