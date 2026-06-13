using MediatR;
using KatyaKatya.Application.Common.Interfaces;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Social;

namespace KatyaKatya.Application.Social.Commands.AnswerFriendRequest;

/// <summary>
/// Handles <see cref="AnswerFriendRequestCommand"/>: accepts or rejects a pending
/// friend request. On acceptance, a <see cref="Friendship"/> is also created.
/// </summary>
public class AnswerFriendRequestCommandHandler : IRequestHandler<AnswerFriendRequestCommand, Unit>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public AnswerFriendRequestCommandHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(AnswerFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = await _socialRepository.GetFriendRequestByIdAsync(request.RequestId)
            ?? throw new DomainException(DomainErrors.Social.FriendRequestNotFound);

        if (request.Accept)
        {
            friendRequest.Accept();
            var friendship = Friendship.Create(friendRequest.SenderId, friendRequest.ReceiverId);
            await _socialRepository.AddFriendshipAsync(friendship);
        }
        else
        {
            friendRequest.Reject();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
