using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Social.Commands.AddSocialNetwork;

/// <summary>
/// Validates <see cref="AddSocialNetworkCommand"/>: ensures the user identifier is valid
/// and the account name is provided and within length limits.
/// </summary>
public class AddSocialNetworkCommandValidator : AbstractValidator<AddSocialNetworkCommand>
{
    /// <summary>
    /// Initializes validation rules for adding a social network.
    /// </summary>
    public AddSocialNetworkCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();

        RuleFor(x => x.Account)
            .NotEmpty().WithMessage("VALIDATION_SOCIAL_ACCOUNT_REQUIRED")
            .MaximumLength(ValidationConstants.SocialAccount.MaxLength)
                .WithMessage("VALIDATION_SOCIAL_ACCOUNT_TOO_LONG");
    }
}
