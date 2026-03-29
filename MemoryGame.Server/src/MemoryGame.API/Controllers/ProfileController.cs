using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Profile.Commands.ChangePassword;
using MemoryGame.Application.Profile.Commands.ChangeUsername;
using MemoryGame.Application.Profile.Commands.UpdateAvatar;
using MemoryGame.Application.Profile.Commands.UpdatePersonalInfo;
using MemoryGame.Application.Profile.Queries.GetProfileQuery;
using MemoryGame.Application.Profile.Queries.GetUserAvatarQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Manages user profile operations: viewing, editing personal info, avatar, and credentials.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public ProfileController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the full profile of the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetProfileByIdQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the profile of a specific user by identifier.
    /// </summary>
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetProfileById(int userId)
    {
        var query = new GetProfileByIdQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the avatar image bytes for a user.
    /// </summary>
    [HttpGet("{userId:int}/avatar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvatar(int userId)
    {
        var query = new GetUserAvatarQuery(userId);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        return File(result, "image/png");
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(GetUserId(), request.CurrentPassword, request.NewPassword);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Changes the authenticated user's username.
    /// </summary>
    [HttpPut("username")]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
    {
        var command = new ChangeUsernameCommand(GetUserId(), request.NewUsername);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates the authenticated user's avatar image.
    /// </summary>
    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarRequest request)
    {
        var command = new UpdateAvatarCommand(GetUserId(), request.AvatarData);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates the authenticated user's name and last name.
    /// </summary>
    [HttpPut("personal-info")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoRequest request)
    {
        var command = new UpdatePersonalInfoCommand(GetUserId(), request.Name, request.LastName);
        await _mediator.Send(command);
        return NoContent();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

/// <summary>
/// Request body for changing the user's password.
/// </summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>
/// Request body for changing the user's username.
/// </summary>
public record ChangeUsernameRequest(string NewUsername);

/// <summary>
/// Request body for updating the user's avatar image.
/// </summary>
public record UpdateAvatarRequest(byte[] AvatarData);

/// <summary>
/// Request body for updating the user's personal information.
/// </summary>
public record UpdatePersonalInfoRequest(string? Name, string? LastName);
