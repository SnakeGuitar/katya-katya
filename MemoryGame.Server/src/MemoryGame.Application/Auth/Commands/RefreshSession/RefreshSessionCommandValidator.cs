using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.RefreshSession;

/// <summary>
/// Validates <see cref="RefreshSessionCommand"/>: ensures the refresh token
/// is provided and the user identifier is valid.
/// </summary>
public class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    /// <summary>
    /// Initializes validation rules for session refresh.
    /// </summary>
    public RefreshSessionCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("VALIDATION_REFRESH_TOKEN_REQUIRED");

        RuleFor(x => x.UserId).ValidId();
    }
}
