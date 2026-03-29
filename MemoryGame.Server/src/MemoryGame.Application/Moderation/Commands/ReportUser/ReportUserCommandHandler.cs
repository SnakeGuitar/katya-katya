using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Common.Enums;
using MemoryGame.Domain.Penalties;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Moderation.Commands.ReportUser;

/// <summary>
/// Handles <see cref="ReportUserCommand"/>: validates that both users exist and
/// issues a warning penalty against the reported user.
/// </summary>
public class ReportUserCommandHandler : IRequestHandler<ReportUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPenaltyRepository _penaltyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public ReportUserCommandHandler(
        IUserRepository userRepository,
        IPenaltyRepository penaltyRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _penaltyRepository = penaltyRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(ReportUserCommand request, CancellationToken cancellationToken)
    {
        _ = await _userRepository.GetByIdAsync(request.ReporterId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        _ = await _userRepository.GetByIdAsync(request.TargetUserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        var penalty = Penalty.Create(
            type: PenaltyType.Warning,
            duration: DateTime.UtcNow.AddDays(1),
            matchId: request.MatchId,
            userId: request.TargetUserId);

        await _penaltyRepository.AddAsync(penalty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
