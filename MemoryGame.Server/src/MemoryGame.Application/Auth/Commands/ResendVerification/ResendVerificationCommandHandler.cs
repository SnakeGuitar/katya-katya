using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Maneja <see cref="ResendVerificationCommand"/>: genera un nuevo PIN,
/// actualiza el registro pendiente y reenvía el email de verificación.
/// </summary>
public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, string>
{
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa el handler con sus dependencias.
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
            ?? throw new DomainException("Registration not found.");

        var newPin = GeneratePin();
        pending.UpdatePin(newPin);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationPinAsync(email.Value, newPin);

        return newPin;
    }

    /// <summary>
    /// Genera un PIN numérico de 6 dígitos.
    /// </summary>
    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
