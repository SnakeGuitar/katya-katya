using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

/// <summary>
/// Handles <see cref="UpgradeGuestCommand"/>: verifies the user is a guest and the email
/// is available, then creates the upgrade pending registration and sends the PIN.
/// </summary>
public class UpgradeGuestCommandHandler : IRequestHandler<UpgradeGuestCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
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
            ?? throw new DomainException(DomainErrors.User.NotFound);

        if (!user.IsGuest)
            throw new DomainException(DomainErrors.User.NotAGuest);

        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException(DomainErrors.Auth.EmailAlreadyInUse);

        var pin = GeneratePin();
        var hashedPassword = _passwordService.Hash(request.Password);

        var pendingUpgrade = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, user.Id);
        await _pendingRegistrationRepository.AddAsync(pendingUpgrade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendGuestUpgradeVerificationAsync(email.Value, pin);

        return pin;
    }

    /// <summary>
    /// Generates a 6-digit numeric PIN.
    /// </summary>
    private static string GeneratePin() =>
        Random.Shared.Next(100000, 999999).ToString();
}
