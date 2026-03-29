using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.FinalizeRegistration;

/// <summary>
/// Handles <see cref="FinalizeRegistrationCommand"/>: validates the PIN, creates the user
/// with a verified email, removes the pending registration, and issues session tokens.
/// </summary>
public class FinalizeRegistrationCommandHandler : IRequestHandler<FinalizeRegistrationCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
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

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(FinalizeRegistrationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException(DomainErrors.Auth.PinInvalid);

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException(DomainErrors.Auth.PinInvalid);

        var user = User.CreateRegistered(
            username: email.Value.Split('@')[0],
            email: request.Email,
            passwordHash: pending.HashedPassword!);

        user.VerifyEmail();

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _pendingRegistrationRepository.Remove(pending);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var session = UserSession.Create(refreshToken, user.Id, TimeSpan.FromDays(7));
        await _userSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
