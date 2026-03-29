using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Social.Commands.SendFriendRequest;

/// <summary>
/// Handles <see cref="SendFriendRequestCommand"/>: validates that no duplicate or
/// existing friendship exists, then persists a new friend request.
/// </summary>
public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public SendFriendRequestCommandHandler(
        IUserRepository userRepository,
        ISocialRepository socialRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var receiver = await _userRepository.GetByUsernameAsync(request.ReceiverUsername)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        var alreadyFriends = await _socialRepository.AreFriendsAsync(request.SenderId, receiver.Id);
        if (alreadyFriends)
            throw new DomainException(DomainErrors.Social.AlreadyFriends);

        var existingRequest = await _socialRepository.GetPendingRequestBetweenAsync(request.SenderId, receiver.Id);
        if (existingRequest is not null)
            throw new DomainException(DomainErrors.Social.FriendRequestAlreadySent);

        var friendRequest = FriendRequest.Create(request.SenderId, receiver.Id);

        await _socialRepository.AddFriendRequestAsync(friendRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
