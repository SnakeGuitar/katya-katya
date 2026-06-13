using FluentValidation;
using KatyaKatya.Application.Common.Validators;

namespace KatyaKatya.Application.Social.Commands.SendFriendRequest;

/// <summary>
/// Validates <see cref="SendFriendRequestCommand"/>: ensures the sender identifier is valid
/// and the receiver username meets format requirements.
/// </summary>
public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    /// <summary>
    /// Initializes validation rules for sending a friend request.
    /// </summary>
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.SenderId).ValidId();
        RuleFor(x => x.ReceiverUsername).ValidUsername();
    }
}
