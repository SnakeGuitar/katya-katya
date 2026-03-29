using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.UpgradeGuest;

/// <summary>
/// Validates <see cref="UpgradeGuestCommand"/>: ensures the guest identifier is valid,
/// the email meets format requirements, and the password meets complexity rules.
/// </summary>
public class UpgradeGuestCommandValidator : AbstractValidator<UpgradeGuestCommand>
{
    /// <summary>
    /// Initializes validation rules for guest account upgrade.
    /// </summary>
    public UpgradeGuestCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
    }
}
