using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Commands.LoginAsGuest;

/// <summary>
/// Handles <see cref="LoginAsGuestCommand"/>: creates a guest account,
/// persists the session, and issues access tokens.
/// </summary>
public class LoginAsGuestCommandHandler : IRequestHandler<LoginAsGuestCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public LoginAsGuestCommandHandler(
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
    public async Task<AuthResponse> Handle(LoginAsGuestCommand request, CancellationToken cancellationToken)
    {
        var user = User.CreateGuest(request.GuestUsername);

        await _userRepository.AddAsync(user);
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
