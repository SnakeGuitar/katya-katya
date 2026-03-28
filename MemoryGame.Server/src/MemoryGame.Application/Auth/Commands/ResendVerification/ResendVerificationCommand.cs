using MediatR;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

public record ResendVerificationCommand(
    string Email
) : IRequest<string>;
