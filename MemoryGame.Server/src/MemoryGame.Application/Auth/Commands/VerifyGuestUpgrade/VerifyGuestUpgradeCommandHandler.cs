using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;

public class VerifyGuestUpgradeCommandHandler : IRequestHandler<VerifyGuestUpgradeCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyGuestUpgradeCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPendingRegistrationRepository pendingRegistrationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _pendingRegistrationRepository = pendingRegistrationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(VerifyGuestUpgradeCommand request, CancellationToken cancellationToken)
    {
        // Obtener usuario guest
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        if (!user.IsGuest)
            throw new DomainException("User is not a guest account.");

        // Validar email del PIN
        var email = Email.Create(request.Email);
        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Invalid or expired PIN.");

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException("Invalid or expired PIN.");

        // Actualizar usuario con email y password
        user.PromoteFromGuest(email.Value, pending.HashedPassword!);
        user.VerifyEmail();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Eliminar registro pendiente
        _pendingRegistrationRepository.Remove(pending);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
