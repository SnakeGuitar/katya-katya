using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Commands.RefreshSession;

/// <summary>
/// Handles <see cref="RefreshSessionCommand"/>: validates the refresh token
/// and issues a new access token without rotating the session.
/// </summary>
public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public RefreshSessionCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUserSessionRepository userSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _userSessionRepository.GetByTokenAsync(request.RefreshToken)
            ?? throw new DomainException(DomainErrors.Auth.RefreshTokenInvalid);

        if (session.UserId != request.UserId || session.IsExpired())
            throw new DomainException(DomainErrors.Auth.RefreshTokenInvalid);

        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: request.RefreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
