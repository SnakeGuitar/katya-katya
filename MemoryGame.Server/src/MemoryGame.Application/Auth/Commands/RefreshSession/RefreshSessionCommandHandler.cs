using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

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

    public async Task<AuthResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        // Validar refresh token
        var session = await _userSessionRepository.GetByTokenAsync(request.RefreshToken)
            ?? throw new DomainException("Invalid refresh token.");

        if (session.UserId != request.UserId || session.IsExpired())
            throw new DomainException("Invalid or expired refresh token.");

        // Obtener usuario
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        // Generar nuevo access token
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: request.RefreshToken,
            User: new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail));
    }
}
