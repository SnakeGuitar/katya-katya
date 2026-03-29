using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Commands.Login;

/// <summary>
/// Handles <see cref="LoginCommand"/>: authenticates the user by verifying credentials
/// and account state, then persists the session and issues tokens.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IUserSessionRepository userSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username)
            ?? throw new DomainException("Invalid username or password.");

        if (user.IsGuest)
            throw new DomainException("Guest accounts cannot login with password.");

        if (!_passwordService.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid username or password.");

        if (!user.VerifiedEmail)
            throw new DomainException("Email not verified. Please verify your email first.");

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
