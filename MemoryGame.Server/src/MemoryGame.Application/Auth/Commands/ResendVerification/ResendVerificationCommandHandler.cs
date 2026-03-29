using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Handles <see cref="ResendVerificationCommand"/>: generates a new PIN,
/// updates the pending registration, and resends the verification email.
/// </summary>
public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, string>
{
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public ResendVerificationCommandHandler(
        IPendingRegistrationRepository pendingRegistrationRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<string> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException(DomainErrors.Auth.RegistrationNotFound);

        var newPin = GeneratePin();
        pending.UpdatePin(newPin);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationPinAsync(email.Value, newPin);

        return newPin;
    }

    /// <summary>
    /// Generates a 6-digit numeric PIN.
    /// </summary>
    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
