using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;

/// <summary>
/// Handles <see cref="VerifyGuestUpgradeCommand"/>: validates the PIN, promotes the guest
/// account to a registered account, removes the pending registration, and issues new tokens.
/// </summary>
public class VerifyGuestUpgradeCommandHandler : IRequestHandler<VerifyGuestUpgradeCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
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

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(VerifyGuestUpgradeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        if (!user.IsGuest)
            throw new DomainException(DomainErrors.User.NotAGuest);

        var email = Email.Create(request.Email);
        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException(DomainErrors.Auth.PinInvalid);

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException(DomainErrors.Auth.PinInvalid);

        user.PromoteFromGuest(email.Value, pending.HashedPassword!);
        user.VerifyEmail();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _pendingRegistrationRepository.Remove(pending);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
