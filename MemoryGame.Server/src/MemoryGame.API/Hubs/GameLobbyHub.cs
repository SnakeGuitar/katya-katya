using System.Security.Claims;
using MemoryGame.Application.Lobbies.DTOs;
using MemoryGame.Application.Lobbies.Interfaces;
using MemoryGame.Application.Lobbies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MemoryGame.API.Hubs;

/// <summary>
/// SignalR hub that replaces the legacy WCF IGameLobbyService and IGameLobbyCallback.
/// Manages lobby lifecycle, chat, game turns, card flipping, and kick voting.
/// </summary>
[Authorize]
public class GameLobbyHub : Hub
{
    private readonly ILobbyManager _lobbyManager;
    private readonly ILogger<GameLobbyHub> _logger;

    /// <summary>
    /// Time players have to see both cards before they are hidden or confirmed as matched.
    /// Intentionally short to keep the game feel snappy.
    /// </summary>
    private static readonly TimeSpan CardRevealDelay = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Initializes the hub with its dependencies.
    /// </summary>
    public GameLobbyHub(ILobbyManager lobbyManager, ILogger<GameLobbyHub> logger)
    {
        _lobbyManager = lobbyManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new lobby and adds the caller as host.
    /// Client event: UpdatePlayerList
    /// </summary>
    public async Task CreateLobby(string gameCode, bool isPublic)
    {
        var lobby = _lobbyManager.CreateLobby(gameCode, isPublic);
        if (lobby is null)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_CODE_TAKEN");
            return;
        }

        var player = CreatePlayerFromContext(isHost: true);
        lobby.TryAddPlayer(player);

        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        await Clients.Caller.SendAsync("LobbyCreated", gameCode);
        await Clients.Group(gameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());
    }

    /// <summary>
    /// Joins an existing lobby by game code.
    /// Client events: PlayerJoined, UpdatePlayerList
    /// </summary>
    public async Task JoinLobby(string gameCode)
    {
        var lobby = _lobbyManager.GetLobby(gameCode);
        if (lobby is null)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_NOT_FOUND");
            return;
        }

        if (lobby.IsGameInProgress)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_GAME_IN_PROGRESS");
            return;
        }

        var player = CreatePlayerFromContext(isHost: false);
        if (!lobby.TryAddPlayer(player))
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_FULL");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        await Clients.Group(gameCode).SendAsync("PlayerJoined", player.Username, player.IsGuest);
        await Clients.Group(gameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());
    }

    /// <summary>
    /// Leaves the current lobby. If the lobby becomes empty, it is destroyed.
    /// Client events: PlayerLeft, UpdatePlayerList
    /// </summary>
    public async Task LeaveLobby()
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        if (lobby is null) return;

        var player = lobby.RemovePlayer(Context.ConnectionId);
        if (player is null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobby.GameCode);
        await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", player.Username);

        if (lobby.Players.Count == 0)
        {
            _lobbyManager.RemoveLobby(lobby.GameCode);
        }
        else
        {
            await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());

            if (lobby.IsGameInProgress && lobby.Game!.IsFinished)
                await NotifyGameFinished(lobby);
        }
    }

    /// <summary>
    /// Sends a chat message to all players in the lobby.
    /// Client event: ReceiveChatMessage
    /// </summary>
    public async Task SendChatMessage(string message)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var player = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || player is null) return;

        if (string.IsNullOrWhiteSpace(message) || message.Length > 500) return;

        await Clients.Group(lobby.GameCode).SendAsync("ReceiveChatMessage", player.Username, message, false);
    }

    /// <summary>
    /// Starts the game. Only the host can call this. Requires at least 2 players.
    /// Client events: GameStarted, UpdateTurn
    /// </summary>
    public async Task StartGame(GameSettingsDto settings)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var player = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || player is null || !player.IsHost) return;

        if (lobby.Players.Count < 2)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_NOT_ENOUGH_PLAYERS");
            return;
        }

        if (settings.CardCount < 4 || settings.CardCount > 36 || settings.CardCount % 2 != 0)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_INVALID_CARD_COUNT");
            return;
        }

        if (settings.TurnTimeSeconds < 5 || settings.TurnTimeSeconds > 120)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_INVALID_TURN_TIME");
            return;
        }

        var game = lobby.StartGame(settings);

        var boardForClients = game.Board
            .Select(c => new CardInfoDto(c.Index, null, false))
            .ToList();

        await Clients.Group(lobby.GameCode).SendAsync("GameStarted", boardForClients);
        await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", game.CurrentPlayer, game.TurnTimeSeconds);
    }

    /// <summary>
    /// Flips a card on the board. Handles match detection with a reveal delay.
    /// Client events: ShowCard, SetCardsAsMatched/HideCards, UpdateScore, UpdateTurn, GameFinished
    /// </summary>
    public async Task FlipCard(int cardIndex)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var player = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby?.Game is null || player is null) return;

        var game = lobby.Game;
        var firstCard = game.FirstFlippedCard;
        var flipped = game.FlipCard(cardIndex, player.Username);

        if (flipped is null) return;

        // Send ShowCard to the caller first for immediate visual feedback,
        // then broadcast to the rest of the group in parallel.
        var notifyOthers = Clients.GroupExcept(lobby.GameCode, Context.ConnectionId)
            .SendAsync("ShowCard", flipped.Index, flipped.ImageIdentifier);
        await Clients.Caller.SendAsync("ShowCard", flipped.Index, flipped.ImageIdentifier);
        await notifyOthers;

        if (firstCard is not null)
        {
            await Task.Delay(CardRevealDelay);

            var isMatch = game.EvaluateMatch(firstCard, flipped);

            if (isMatch)
            {
                await Clients.Group(lobby.GameCode).SendAsync("SetCardsAsMatched", firstCard.Index, flipped.Index);
                await Clients.Group(lobby.GameCode).SendAsync("UpdateScore", player.Username, game.Scores[player.Username]);

                if (game.IsFinished)
                {
                    await NotifyGameFinished(lobby);
                    return;
                }
            }
            else
            {
                await Clients.Group(lobby.GameCode).SendAsync("HideCards", firstCard.Index, flipped.Index);
            }

            await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", game.CurrentPlayer, game.TurnTimeSeconds);
        }
    }

    /// <summary>
    /// Votes to kick a player. If majority is reached, the player is removed.
    /// Client events: ReceiveChatMessage (notification), PlayerLeft, UpdatePlayerList
    /// </summary>
    public async Task VoteToKick(string targetUsername)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var voter = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || voter is null) return;

        var target = lobby.Players.Values.FirstOrDefault(p => p.Username == targetUsername);
        if (target is null || target.ConnectionId == Context.ConnectionId) return;

        var shouldKick = lobby.VoteToKick(voter.Username, targetUsername);

        await Clients.Group(lobby.GameCode).SendAsync(
            "ReceiveChatMessage", "System",
            $"{voter.Username} voted to kick {targetUsername}.", true);

        if (shouldKick)
        {
            lobby.RemovePlayer(target.ConnectionId);
            await Groups.RemoveFromGroupAsync(target.ConnectionId, lobby.GameCode);
            await Clients.Client(target.ConnectionId).SendAsync("Kicked");
            await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", targetUsername);
            await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());
            await Clients.Group(lobby.GameCode).SendAsync(
                "ReceiveChatMessage", "System",
                $"{targetUsername} has been kicked.", true);

            if (lobby.IsGameInProgress && lobby.Game!.IsFinished)
                await NotifyGameFinished(lobby);
        }
    }

    /// <summary>
    /// Returns a list of public, non-full lobbies that are not in a game.
    /// Client event: PublicLobbiesList
    /// </summary>
    public async Task GetPublicLobbies()
    {
        var lobbies = _lobbyManager.GetPublicLobbies();
        await Clients.Caller.SendAsync("PublicLobbiesList", lobbies);
    }

    /// <summary>
    /// Handles unexpected disconnections by removing the player from their lobby.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        if (lobby is not null)
        {
            var player = lobby.RemovePlayer(Context.ConnectionId);
            if (player is not null)
            {
                await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", player.Username);

                if (lobby.Players.Count == 0)
                {
                    _lobbyManager.RemoveLobby(lobby.GameCode);
                }
                else
                {
                    await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());
                    if (lobby.IsGameInProgress && lobby.Game!.IsFinished)
                        await NotifyGameFinished(lobby);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task NotifyGameFinished(Lobby lobby)
    {
        var winner = lobby.Game!.GetWinner();
        await Clients.Group(lobby.GameCode).SendAsync("GameFinished", winner);
    }

    private LobbyPlayer CreatePlayerFromContext(bool isHost)
    {
        var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var username = Context.User?.FindFirst("username")?.Value ?? "Unknown";
        var isGuest = Context.User?.FindFirst("isGuest")?.Value == "true";

        return new LobbyPlayer(Context.ConnectionId, userId, username, isGuest, isHost);
    }
}
