using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.Register;

/// <summary>
/// Maneja <see cref="RegisterCommand"/>: valida unicidad de email y username,
/// crea el registro pendiente con PIN y envía el email de verificación.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa el handler con sus dependencias.
    /// </summary>
    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPendingRegistrationRepository pendingRegistrationRepository,
        IPasswordService passwordService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _passwordService = passwordService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await _userRepository.ExistsByEmailAsync(email))
            throw new DomainException("Email already registered.");

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new DomainException("Username already taken.");

        var pin = GeneratePin();
        var passwordHash = _passwordService.Hash(request.Password);

        var pendingRegistration = PendingRegistration.Create(
            email: request.Email,
            pin: pin,
            hashedPassword: passwordHash,
            validity: TimeSpan.FromMinutes(15));

        await _pendingRegistrationRepository.AddAsync(pendingRegistration);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationPinAsync(request.Email, pin);

        return pin;
    }

    /// <summary>
    /// Genera un PIN numérico de 6 dígitos.
    /// </summary>
    private static string GeneratePin()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
