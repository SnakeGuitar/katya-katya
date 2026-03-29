using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Social.Commands.RemoveSocialNetwork;

/// <summary>
/// Validates <see cref="RemoveSocialNetworkCommand"/>: ensures both identifiers are valid.
/// </summary>
public class RemoveSocialNetworkCommandValidator : AbstractValidator<RemoveSocialNetworkCommand>
{
    /// <summary>
    /// Initializes validation rules for removing a social network.
    /// </summary>
    public RemoveSocialNetworkCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.SocialNetworkId).ValidId();
    }
}
