using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Moderation.Commands.ReportUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Handles user moderation operations such as reporting misconduct.
/// </summary>
[ApiController]
[Route("api/moderation")]
[Authorize]
public class ModerationController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public ModerationController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Reports a user for misconduct during a match.
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> ReportUser([FromBody] ReportUserRequest request)
    {
        var command = new ReportUserCommand(GetUserId(), request.TargetUserId, request.MatchId);
        await _mediator.Send(command);
        return NoContent();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

/// <summary>
/// Request body for reporting a user.
/// </summary>
public record ReportUserRequest(int TargetUserId, int MatchId);
