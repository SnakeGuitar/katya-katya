using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Social.Commands.RemoveFriend;

/// <summary>
/// Validates <see cref="RemoveFriendCommand"/>: ensures both identifiers are valid
/// and the user is not attempting to remove themselves.
/// </summary>
public class RemoveFriendCommandValidator : AbstractValidator<RemoveFriendCommand>
{
    /// <summary>
    /// Initializes validation rules for friend removal.
    /// </summary>
    public RemoveFriendCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.FriendId).ValidId();

        RuleFor(x => x)
            .Must(x => x.UserId != x.FriendId)
                .WithMessage("VALIDATION_CANNOT_REMOVE_SELF");
    }
}
