using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Profile.Commands.ChangeUsername;

/// <summary>
/// Validates <see cref="ChangeUsernameCommand"/>: ensures the user identifier is valid
/// and the new username meets format and length requirements.
/// </summary>
public class ChangeUsernameCommandValidator : AbstractValidator<ChangeUsernameCommand>
{
    /// <summary>
    /// Initializes validation rules for username change.
    /// </summary>
    public ChangeUsernameCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.NewUsername).ValidUsername();
    }
}
