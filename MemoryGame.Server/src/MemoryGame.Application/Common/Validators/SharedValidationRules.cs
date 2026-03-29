using FluentValidation;

namespace MemoryGame.Application.Common.Validators;

/// <summary>
/// Reusable FluentValidation rule extensions for common field validations.
/// </summary>
public static class SharedValidationRules
{
    /// <summary>
    /// Applies standard email validation rules: not empty, max length, and regex format.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("VALIDATION_EMAIL_REQUIRED")
            .MaximumLength(ValidationConstants.Email.MaxLength).WithMessage("VALIDATION_EMAIL_TOO_LONG")
            .Matches(ValidationConstants.Email.Pattern).WithMessage("VALIDATION_EMAIL_INVALID_FORMAT");
    }

    /// <summary>
    /// Applies standard password validation rules: not empty, length range, and complexity
    /// (uppercase, lowercase, digit, and special character).
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("VALIDATION_PASSWORD_REQUIRED")
            .MinimumLength(ValidationConstants.Password.MinLength).WithMessage("VALIDATION_PASSWORD_TOO_SHORT")
            .MaximumLength(ValidationConstants.Password.MaxLength).WithMessage("VALIDATION_PASSWORD_TOO_LONG")
            .Must(ContainsUppercase).WithMessage("VALIDATION_PASSWORD_REQUIRES_UPPERCASE")
            .Must(ContainsLowercase).WithMessage("VALIDATION_PASSWORD_REQUIRES_LOWERCASE")
            .Must(ContainsDigit).WithMessage("VALIDATION_PASSWORD_REQUIRES_DIGIT")
            .Must(ContainsSpecialCharacter).WithMessage("VALIDATION_PASSWORD_REQUIRES_SPECIAL");
    }

    /// <summary>
    /// Applies standard username validation rules: not empty, length range, and allowed characters
    /// (letters, digits, underscores, hyphens).
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidUsername<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("VALIDATION_USERNAME_REQUIRED")
            .MinimumLength(ValidationConstants.Username.MinLength).WithMessage("VALIDATION_USERNAME_TOO_SHORT")
            .MaximumLength(ValidationConstants.Username.MaxLength).WithMessage("VALIDATION_USERNAME_TOO_LONG")
            .Matches(ValidationConstants.Username.Pattern).WithMessage("VALIDATION_USERNAME_INVALID_CHARACTERS");
    }

    /// <summary>
    /// Applies standard PIN validation rules: not empty, exactly 6 digits.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPin<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("VALIDATION_PIN_REQUIRED")
            .Matches(ValidationConstants.Pin.Pattern).WithMessage("VALIDATION_PIN_INVALID_FORMAT");
    }

    /// <summary>
    /// Applies a positive integer validation rule: must be greater than zero.
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidId<T>(this IRuleBuilder<T, int> rule)
    {
        return rule
            .GreaterThan(0).WithMessage("VALIDATION_ID_INVALID");
    }

    private static bool ContainsUppercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool ContainsLowercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool ContainsDigit(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);

    private static bool ContainsSpecialCharacter(string password) =>
        !string.IsNullOrEmpty(password) && !password.All(char.IsLetterOrDigit);
}
