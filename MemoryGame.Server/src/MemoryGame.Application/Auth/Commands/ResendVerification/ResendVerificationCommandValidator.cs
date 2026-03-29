using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Validates <see cref="ResendVerificationCommand"/>: ensures the email format is valid.
/// </summary>
public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    /// <summary>
    /// Initializes validation rules for verification resend.
    /// </summary>
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}
