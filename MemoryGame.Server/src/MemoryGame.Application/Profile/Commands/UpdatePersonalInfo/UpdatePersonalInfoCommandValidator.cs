using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Profile.Commands.UpdatePersonalInfo;

/// <summary>
/// Validates <see cref="UpdatePersonalInfoCommand"/>: ensures the user identifier is valid
/// and optional name fields do not exceed the maximum length.
/// </summary>
public class UpdatePersonalInfoCommandValidator : AbstractValidator<UpdatePersonalInfoCommand>
{
    /// <summary>
    /// Initializes validation rules for personal info update.
    /// </summary>
    public UpdatePersonalInfoCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();

        RuleFor(x => x.Name)
            .MaximumLength(ValidationConstants.PersonalInfo.NameMaxLength)
                .WithMessage("VALIDATION_NAME_TOO_LONG")
                .When(x => x.Name is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(ValidationConstants.PersonalInfo.NameMaxLength)
                .WithMessage("VALIDATION_LAST_NAME_TOO_LONG")
                .When(x => x.LastName is not null);
    }
}
