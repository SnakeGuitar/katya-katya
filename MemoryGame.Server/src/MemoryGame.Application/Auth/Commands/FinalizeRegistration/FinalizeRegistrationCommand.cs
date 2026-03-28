using MediatR;
using MemoryGame.Application.Auth.DTOs;

namespace MemoryGame.Application.Auth.Commands.FinalizeRegistration;

public record FinalizeRegistrationCommand(
    string Email,
    string Pin
) : IRequest<AuthResponse>;
