using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Moderation.Commands.ReportUser;

/// <summary>
/// Validates <see cref="ReportUserCommand"/>: ensures all identifiers are valid
/// and the reporter is not reporting themselves.
/// </summary>
public class ReportUserCommandValidator : AbstractValidator<ReportUserCommand>
{
    /// <summary>
    /// Initializes validation rules for user reporting.
    /// </summary>
    public ReportUserCommandValidator()
    {
        RuleFor(x => x.ReporterId).ValidId();
        RuleFor(x => x.TargetUserId).ValidId();
        RuleFor(x => x.MatchId).ValidId();

        RuleFor(x => x)
            .Must(x => x.ReporterId != x.TargetUserId)
                .WithMessage("VALIDATION_CANNOT_REPORT_SELF");
    }
}
