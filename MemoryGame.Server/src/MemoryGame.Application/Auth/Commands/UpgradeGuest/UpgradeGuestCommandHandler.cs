using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

/// <summary>
/// Maneja <see cref="UpgradeGuestCommand"/>: valida que el usuario sea guest y el email
/// esté disponible, luego crea el registro pendiente de upgrade y envía el PIN.
/// </summary>
public class UpgradeGuestCommandHandler : IRequestHandler<UpgradeGuestCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa el handler con sus dependencias.
    /// </summary>
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

    /// <inheritdoc/>
    public async Task<string> Handle(UpgradeGuestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        if (!user.IsGuest)
            throw new DomainException("User is not a guest account.");

        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException("Email already in use.");

        var pin = GeneratePin();
        var hashedPassword = _passwordService.Hash(request.Password);

        var pendingUpgrade = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, user.Id);
        await _pendingRegistrationRepository.AddAsync(pendingUpgrade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendGuestUpgradeVerificationAsync(email.Value, pin);

        return pin;
    }

    /// <summary>
    /// Genera un PIN numérico de 6 dígitos.
    /// </summary>
    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
