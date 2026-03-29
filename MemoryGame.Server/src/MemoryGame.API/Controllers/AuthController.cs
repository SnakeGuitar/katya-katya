using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Auth.Commands.FinalizeRegistration;
using MemoryGame.Application.Auth.Commands.Login;
using MemoryGame.Application.Auth.Commands.LoginAsGuest;
using MemoryGame.Application.Auth.Commands.Logout;
using MemoryGame.Application.Auth.Commands.RefreshSession;
using MemoryGame.Application.Auth.Commands.Register;
using MemoryGame.Application.Auth.Commands.ResendVerification;
using MemoryGame.Application.Auth.Commands.UpgradeGuest;
using MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;
using MemoryGame.Application.Auth.Commands.VerifyRegistration;
using MemoryGame.Application.Auth.Queries.GetUserByIdQuery;
using MemoryGame.Application.Auth.Queries.GetUserByUsernameQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Handles authentication, registration, session management, and guest account operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Starts the registration process by sending a verification PIN to the email.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var pin = await _mediator.Send(command);
        return Ok(new { Message = pin });
    }

    /// <summary>
    /// Validates the registration PIN without creating the user account.
    /// </summary>
    [HttpPost("verify-registration")]
    public async Task<IActionResult> VerifyRegistration([FromBody] VerifyRegistrationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Valid = result });
    }

    /// <summary>
    /// Completes the registration by validating the PIN and creating the user account.
    /// </summary>
    [HttpPost("finalize-registration")]
    public async Task<IActionResult> FinalizeRegistration([FromBody] FinalizeRegistrationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a registered user with username and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Creates and authenticates a temporary guest account.
    /// </summary>
    [HttpPost("login-guest")]
    public async Task<IActionResult> LoginAsGuest([FromBody] LoginAsGuestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Ends the authenticated user's session by revoking refresh tokens.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand(GetUserId());
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-session")]
    public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionRequest request)
    {
        var command = new RefreshSessionCommand(request.RefreshToken, request.UserId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Resends the verification PIN to the given email address.
    /// </summary>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationCommand command)
    {
        var pin = await _mediator.Send(command);
        return Ok(new { Message = pin });
    }

    /// <summary>
    /// Starts converting a guest account into a registered account.
    /// </summary>
    [Authorize]
    [HttpPost("upgrade-guest")]
    public async Task<IActionResult> UpgradeGuest([FromBody] UpgradeGuestRequest request)
    {
        var command = new UpgradeGuestCommand(GetUserId(), request.Email, request.Password);
        var pin = await _mediator.Send(command);
        return Ok(new { Message = pin });
    }

    /// <summary>
    /// Completes the guest-to-registered conversion by validating the PIN.
    /// </summary>
    [Authorize]
    [HttpPost("verify-guest-upgrade")]
    public async Task<IActionResult> VerifyGuestUpgrade([FromBody] VerifyGuestUpgradeRequest request)
    {
        var command = new VerifyGuestUpgradeCommand(GetUserId(), request.Email, request.Pin);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their identifier.
    /// </summary>
    [Authorize]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    [Authorize]
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var query = new GetUserByUsernameQuery(username);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

/// <summary>
/// Request body for session refresh.
/// </summary>
public record RefreshSessionRequest(string RefreshToken, int UserId);

/// <summary>
/// Request body for initiating a guest account upgrade.
/// </summary>
public record UpgradeGuestRequest(string Email, string Password);

/// <summary>
/// Request body for verifying a guest account upgrade.
/// </summary>
public record VerifyGuestUpgradeRequest(string Email, string Pin);
