using MediatR;

namespace MemoryGame.Application.Auth.Commands.VerifyRegistration;

public record VerifyRegistrationCommand(
    string Email,
    string Pin
) : IRequest<bool>;
