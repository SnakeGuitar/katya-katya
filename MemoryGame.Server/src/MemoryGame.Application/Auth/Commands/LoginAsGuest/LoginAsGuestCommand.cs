using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.LoginAsGuest;

public record LoginAsGuestCommand(
    string GuestUsername
) : IRequest<AuthResponse>;
