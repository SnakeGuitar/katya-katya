using MediatR;

namespace MemoryGame.Application.Moderation.Commands.ReportUser;

/// <summary>
/// Reports a user for misconduct during a match, issuing a warning penalty.
/// </summary>
/// <param name="ReporterId">The identifier of the user filing the report.</param>
/// <param name="TargetUserId">The identifier of the user being reported.</param>
/// <param name="MatchId">The identifier of the match where the misconduct occurred.</param>
public record ReportUserCommand(int ReporterId, int TargetUserId, int MatchId) : IRequest<Unit>;
