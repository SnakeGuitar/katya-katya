using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Matches.Queries.GetMatchHistoryQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Provides access to match history and match-related queries.
/// </summary>
[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public MatchesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the match history for the authenticated user.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetMatchHistory()
    {
        var query = new GetMatchHistoryQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}
