using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Commands.Logout;

/// <summary>
/// Handles <see cref="LogoutCommand"/>: revokes all active sessions for the user.
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public LogoutCommandHandler(IUserSessionRepository userSessionRepository, IUnitOfWork unitOfWork)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var sessions = await _userSessionRepository.GetByUserIdAsync(request.UserId);
        foreach (var session in sessions)
        {
            _userSessionRepository.Remove(session);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
