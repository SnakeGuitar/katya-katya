using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.VerifyRegistration;

/// <summary>
/// Handles <see cref="VerifyRegistrationCommand"/>: validates that the PIN is correct
/// and has not expired without consuming the pending registration.
/// </summary>
public class VerifyRegistrationCommandHandler : IRequestHandler<VerifyRegistrationCommand, bool>
{
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public VerifyRegistrationCommandHandler(IPendingRegistrationRepository pendingRegistrationRepository)
    {
        _pendingRegistrationRepository = pendingRegistrationRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException(DomainErrors.Auth.RegistrationNotFound);

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException(DomainErrors.Auth.PinInvalid);

        return true;
    }
}
