using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, string>
{
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ResendVerificationCommandHandler(
        IPendingRegistrationRepository pendingRegistrationRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        // Recuperar registro pendiente
        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Registration not found.");

        // Generar nuevo PIN
        var newPin = GeneratePin();
        pending.UpdatePin(newPin);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviar email con PIN
        await _emailService.SendVerificationPinAsync(email.Value, newPin);

        // TODO: En producción, solo devolver "PIN enviado" sin el PIN real
        return newPin;
    }

    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
