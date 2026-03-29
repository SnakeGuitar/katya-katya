using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;

namespace MemoryGame.Application.Social.Commands.RemoveFriend;

/// <summary>
/// Handles <see cref="RemoveFriendCommand"/>: verifies the friendship exists
/// and removes it from both sides.
/// </summary>
public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, Unit>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public RemoveFriendCommandHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var areFriends = await _socialRepository.AreFriendsAsync(request.UserId, request.FriendId);
        if (!areFriends)
            throw new DomainException(DomainErrors.Social.NotFriends);

        var friendships = await _socialRepository.GetFriendsAsync(request.UserId);
        var friendship = friendships.FirstOrDefault(f =>
            (f.UserId == request.UserId && f.FriendId == request.FriendId) ||
            (f.UserId == request.FriendId && f.FriendId == request.UserId))
            ?? throw new DomainException(DomainErrors.Social.NotFriends);

        _socialRepository.RemoveFriendship(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
