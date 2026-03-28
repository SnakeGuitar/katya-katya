using MediatR;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

public record UpgradeGuestCommand(
    int UserId,
    string Email,
    string Password
) : IRequest<string>;
