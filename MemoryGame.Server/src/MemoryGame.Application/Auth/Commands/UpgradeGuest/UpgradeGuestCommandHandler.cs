using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

public class UpgradeGuestCommandHandler : IRequestHandler<UpgradeGuestCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeGuestCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IPendingRegistrationRepository pendingRegistrationRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(UpgradeGuestCommand request, CancellationToken cancellationToken)
    {
        // Obtener usuario guest
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        if (!user.IsGuest)
            throw new DomainException("User is not a guest account.");

        // Validar email no exista
        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException("Email already in use.");

        // Generar PIN para verificación
        var pin = GeneratePin();
        var hashedPassword = _passwordService.Hash(request.Password);

        // Crear registro pendiente de upgrade
        var pendingUpgrade = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, user.Id);
        await _pendingRegistrationRepository.AddAsync(pendingUpgrade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviar email de verificación
        await _emailService.SendGuestUpgradeVerificationAsync(email.Value, pin);

        // TODO: En producción, solo devolver "PIN enviado"
        return pin;
    }

    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
