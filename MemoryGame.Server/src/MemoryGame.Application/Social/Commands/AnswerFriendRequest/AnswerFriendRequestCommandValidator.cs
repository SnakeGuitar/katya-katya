using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Social.Commands.AnswerFriendRequest;

/// <summary>
/// Validates <see cref="AnswerFriendRequestCommand"/>: ensures both identifiers are valid.
/// </summary>
public class AnswerFriendRequestCommandValidator : AbstractValidator<AnswerFriendRequestCommand>
{
    /// <summary>
    /// Initializes validation rules for answering a friend request.
    /// </summary>
    public AnswerFriendRequestCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.RequestId).ValidId();
    }
}
