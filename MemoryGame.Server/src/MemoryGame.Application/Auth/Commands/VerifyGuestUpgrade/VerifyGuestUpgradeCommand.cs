using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;

public record VerifyGuestUpgradeCommand(
    int UserId,
    string Email,
    string Pin
) : IRequest<AuthResponse>;
