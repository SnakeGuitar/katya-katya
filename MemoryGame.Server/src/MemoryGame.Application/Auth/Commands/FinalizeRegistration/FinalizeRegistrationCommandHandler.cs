using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.FinalizeRegistration;

public class FinalizeRegistrationCommandHandler : IRequestHandler<FinalizeRegistrationCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizeRegistrationCommandHandler(
        IUserRepository userRepository,
        IPendingRegistrationRepository pendingRegistrationRepository,
        IUserSessionRepository userSessionRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _userSessionRepository = userSessionRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(FinalizeRegistrationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        // Recuperar y validar PendingRegistration
        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Invalid or expired PIN.");

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException("Invalid or expired PIN.");

        // Crear usuario
        var user = User.CreateRegistered(
            username: email.Value.Split('@')[0], // username = parte del email
            email: request.Email,
            passwordHash: pending.HashedPassword!);

        user.VerifyEmail();

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Eliminar registro pendiente
        _pendingRegistrationRepository.Remove(pending);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Guardar refreshToken en base de datos (UserSession)
        var session = UserSession.Create(refreshToken, user.Id, TimeSpan.FromDays(7));
        await _userSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
