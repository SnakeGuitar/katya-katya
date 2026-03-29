using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Social.Commands.AddSocialNetwork;
using MemoryGame.Application.Social.Commands.AnswerFriendRequest;
using MemoryGame.Application.Social.Commands.RemoveFriend;
using MemoryGame.Application.Social.Commands.RemoveSocialNetwork;
using MemoryGame.Application.Social.Commands.SendFriendRequest;
using MemoryGame.Application.Social.Queries.GetFriendsListQuery;
using MemoryGame.Application.Social.Queries.GetPendingFriendRequestsQuery;
using MemoryGame.Application.Social.Queries.GetSocialNetworksQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Manages social features: social networks, friend lists, and friend requests.
/// </summary>
[ApiController]
[Route("api/social")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public SocialController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all social network accounts linked to the authenticated user.
    /// </summary>
    [HttpGet("networks")]
    public async Task<IActionResult> GetSocialNetworks()
    {
        var query = new GetSocialNetworksQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Links a new social network account to the authenticated user's profile.
    /// </summary>
    [HttpPost("networks")]
    public async Task<IActionResult> AddSocialNetwork([FromBody] AddSocialNetworkRequest request)
    {
        var command = new AddSocialNetworkCommand(GetUserId(), request.Account);
        var result = await _mediator.Send(command);
        return Created($"api/social/networks/{result.Id}", result);
    }

    /// <summary>
    /// Removes a social network account from the authenticated user's profile.
    /// </summary>
    [HttpDelete("networks/{socialNetworkId:int}")]
    public async Task<IActionResult> RemoveSocialNetwork(int socialNetworkId)
    {
        var command = new RemoveSocialNetworkCommand(GetUserId(), socialNetworkId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Retrieves the authenticated user's friend list.
    /// </summary>
    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var query = new GetFriendsListQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Sends a friend request to another user by username.
    /// </summary>
    [HttpPost("friends/request")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
    {
        var command = new SendFriendRequestCommand(GetUserId(), request.ReceiverUsername);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Retrieves all pending friend requests received by the authenticated user.
    /// </summary>
    [HttpGet("friends/requests")]
    public async Task<IActionResult> GetPendingFriendRequests()
    {
        var query = new GetPendingFriendRequestsQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Accepts or rejects a pending friend request.
    /// </summary>
    [HttpPost("friends/request/answer")]
    public async Task<IActionResult> AnswerFriendRequest([FromBody] AnswerFriendRequestRequest request)
    {
        var command = new AnswerFriendRequestCommand(GetUserId(), request.RequestId, request.Accept);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Removes a friend from the authenticated user's friend list.
    /// </summary>
    [HttpDelete("friends/{friendId:int}")]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        var command = new RemoveFriendCommand(GetUserId(), friendId);
        await _mediator.Send(command);
        return NoContent();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

/// <summary>
/// Request body for adding a social network account.
/// </summary>
public record AddSocialNetworkRequest(string Account);

/// <summary>
/// Request body for sending a friend request.
/// </summary>
public record SendFriendRequestRequest(string ReceiverUsername);

/// <summary>
/// Request body for answering a friend request.
/// </summary>
public record AnswerFriendRequestRequest(int RequestId, bool Accept);
