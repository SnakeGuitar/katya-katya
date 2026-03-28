using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

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

    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validar que email no exista
        var email = Email.Create(request.Email);
        if (await _userRepository.ExistsByEmailAsync(email))
            throw new DomainException("Email already registered.");

        // Validar que username no exista
        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new DomainException("Username already taken.");

        // Generar PIN de verificación
        var pin = GeneratePin();
        var passwordHash = _passwordService.Hash(request.Password);

        // Crear registro pendiente
        var pendingRegistration = PendingRegistration.Create(
            email: request.Email,
            pin: pin,
            hashedPassword: passwordHash,
            validity: TimeSpan.FromMinutes(15));

        // Guardar registro pendiente
        await _pendingRegistrationRepository.AddAsync(pendingRegistration);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviar email con PIN
        await _emailService.SendVerificationPinAsync(request.Email, pin);

        // TODO: En producción, no retornarías el PIN, solo confirmarías el envío
        return pin;
    }

    private static string GeneratePin()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
