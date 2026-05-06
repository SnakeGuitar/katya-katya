using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Application.Lobbies.DTOs;
using MemoryGame.Application.Lobbies.Interfaces;
using MemoryGame.Application.Lobbies.Models;
using MemoryGame.Domain.Matches;
using MemoryGame.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MemoryGame.API.Hubs;

/// <summary>
/// SignalR hub that replaces the legacy WCF IGameLobbyService and IGameLobbyCallback.
/// Manages lobby lifecycle, chat, game turns, card flipping, kick voting, and friend invitations.
/// </summary>
[Authorize]
public class GameLobbyHub : Hub
{
    private readonly ILobbyManager _lobbyManager;
    private readonly IPresenceTracker _presenceTracker;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<GameLobbyHub> _logger;
    private readonly IHubContext<GameLobbyHub> _hubContext;
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly TimeSpan CardRevealDelay = TimeSpan.FromMilliseconds(1000);
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _turnTimers = new();

    public GameLobbyHub(
        ILobbyManager lobbyManager,
        IPresenceTracker presenceTracker,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<GameLobbyHub> logger,
        IHubContext<GameLobbyHub> hubContext,
        IMatchRepository matchRepository,
        IUnitOfWork unitOfWork)
    {
        _lobbyManager     = lobbyManager;
        _presenceTracker  = presenceTracker;
        _userRepository   = userRepository;
        _emailService     = emailService;
        _logger           = logger;
        _hubContext       = hubContext;
        _matchRepository  = matchRepository;
        _unitOfWork       = unitOfWork;
    }

    public async Task CreateLobby(string gameCode, bool isPublic)
    {
        var lobby = _lobbyManager.CreateLobby(gameCode, isPublic);
        if (lobby is null)
        {
            await Clients.Caller.SendAsync("Error", "LOBBY_CODE_TAKEN");
            return;
        }

        var player = CreatePlayerFromContext(isHost: true);
        if (!lobby.TryAddPlayer(player))
        {
            _lobbyManager.RemoveLobby(gameCode);
            await Clients.Caller.SendAsync("Error", "LOBBY_CREATE_FAILED");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        await Clients.Caller.SendAsync("LobbyCreated", gameCode);
        await Clients.Group(gameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        if (lobby is not null)
        {
            var player = lobby.RemovePlayer(Context.ConnectionId);
            if (player is not null)
            {
                CancelTurnTimer(lobby.GameCode);
                await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", player.Username);

                if (lobby.Players.Count == 0)
                {
                    _lobbyManager.RemoveLobby(lobby.GameCode);
                }
                else
                {
                    await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());

                    if (lobby.IsGameInProgress)
                    {
                        if (lobby.Game!.IsFinished)
                            await NotifyGameFinished(lobby);
                        else
                        {
                            await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", lobby.Game.CurrentPlayer, lobby.Game.TurnTimeSeconds);
                            StartTurnTimer(lobby, lobby.Game.TurnTimeSeconds);
                        }
                    }
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

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

    public async Task LeaveLobby()
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        if (lobby is null) return;

        var player = lobby.RemovePlayer(Context.ConnectionId);
        if (player is null) return;

        CancelTurnTimer(lobby.GameCode);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobby.GameCode);
        await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", player.Username);

        if (lobby.Players.Count == 0)
        {
            _lobbyManager.RemoveLobby(lobby.GameCode);
        }
        else
        {
            await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());

            if (lobby.IsGameInProgress)
            {
                if (lobby.Game!.IsFinished)
                    await NotifyGameFinished(lobby);
                else
                {
                    // Update turn in case the current player left
                    await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", lobby.Game.CurrentPlayer, lobby.Game.TurnTimeSeconds);
                    StartTurnTimer(lobby, lobby.Game.TurnTimeSeconds);
                }
            }
        }
    }

    public async Task SendChatMessage(string message)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var player = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || player is null) return;

        if (string.IsNullOrWhiteSpace(message) || message.Length > 500) return;

        await Clients.Group(lobby.GameCode).SendAsync("ReceiveChatMessage", player.Username, message, false);
    }

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

        var game = lobby.StartGame(settings);

        var boardForClients = game.Board
            .Select(c => new CardInfoDto(c.Index, null, false))
            .ToList();

        await Clients.Group(lobby.GameCode).SendAsync("GameStarted", boardForClients);

        // Delay to allow clients to transition
        await Task.Delay(2000);
        await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", game.CurrentPlayer, game.TurnTimeSeconds);
        StartTurnTimer(lobby, game.TurnTimeSeconds);
    }

    public async Task FlipCard(int cardIndex)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var player = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby?.Game is null || player is null) return;

        var game = lobby.Game;
        var firstCard = game.FirstFlippedCard;
        var flipped = game.FlipCard(cardIndex, player.Username);

        if (flipped is null) return;

        // Reset turn timer on activity
        StartTurnTimer(lobby, game.TurnTimeSeconds);

        await Clients.Group(lobby.GameCode).SendAsync("ShowCard", flipped.Index, flipped.ImageIdentifier);

        if (firstCard is not null)
        {
            CancelTurnTimer(lobby.GameCode); // Pause during reveal
            await Task.Delay(CardRevealDelay);

            var isMatch = game.EvaluateMatch(firstCard, flipped);

            if (isMatch)
            {
                await Clients.Group(lobby.GameCode).SendAsync("CardsMatched", firstCard.Index, flipped.Index);
                await Clients.Group(lobby.GameCode).SendAsync("UpdateScore", player.Username, game.Scores[player.Username]);

                if (game.IsFinished)
                {
                    await NotifyGameFinished(lobby);
                    return;
                }
            }
            else
            {
                await Clients.Group(lobby.GameCode).SendAsync("CardsHidden", firstCard.Index, flipped.Index);
            }

            await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", game.CurrentPlayer, game.TurnTimeSeconds);
            StartTurnTimer(lobby, game.TurnTimeSeconds);
        }
    }

    public async Task GetPublicLobbies()
    {
        var lobbies = _lobbyManager.GetPublicLobbies();
        await Clients.Caller.SendAsync("PublicLobbiesList", lobbies);
    }

    public async Task VoteToKick(string targetUsername)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var voter = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || voter is null) return;

        var target = lobby.Players.Values.FirstOrDefault(p => p.Username == targetUsername);
        if (target is null || target.ConnectionId == Context.ConnectionId) return;

        var shouldKick = lobby.VoteToKick(voter.Username, targetUsername);
        await Clients.Group(lobby.GameCode).SendAsync("ReceiveChatMessage", "System", $"{voter.Username} voted to kick {targetUsername}.", true);

        if (shouldKick)
        {
            await PerformKick(lobby, target);
        }
    }

    public async Task KickPlayer(string targetUsername)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var host = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || host is null || !host.IsHost) return;

        var target = lobby.Players.Values.FirstOrDefault(p => p.Username == targetUsername);
        if (target is null || target.IsHost) return;

        await PerformKick(lobby, target);
    }

    private async Task PerformKick(Lobby lobby, LobbyPlayer target)
    {
        await Clients.Client(target.ConnectionId).SendAsync("Kicked");
        lobby.RemovePlayer(target.ConnectionId);
        await Groups.RemoveFromGroupAsync(target.ConnectionId, lobby.GameCode);
        await Clients.Group(lobby.GameCode).SendAsync("PlayerLeft", target.Username);
        await Clients.Group(lobby.GameCode).SendAsync("UpdatePlayerList", lobby.GetPlayerList());

        if (lobby.IsGameInProgress)
        {
            if (lobby.Game!.IsFinished)
                await NotifyGameFinished(lobby);
            else
            {
                await Clients.Group(lobby.GameCode).SendAsync("UpdateTurn", lobby.Game.CurrentPlayer, lobby.Game.TurnTimeSeconds);
                StartTurnTimer(lobby, lobby.Game.TurnTimeSeconds);
            }
        }
    }

    public async Task InviteFriend(int targetUserId)
    {
        var lobby = _lobbyManager.FindLobbyByConnection(Context.ConnectionId);
        var sender = lobby?.GetPlayer(Context.ConnectionId);
        if (lobby is null || sender is null) return;

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser is null) return;

        var targetConnection = _presenceTracker.GetConnectionId(targetUser.Id);
        if (targetConnection is not null)
        {
            await Clients.Client(targetConnection).SendAsync("LobbyInviteReceived", sender.Username, lobby.GameCode);
            await Clients.Caller.SendAsync("LobbyInviteSent", targetUser.Username, true);
        }
        else if (targetUser.Email is not null)
        {
            await _emailService.SendLobbyInviteAsync(targetUser.Email.Value, targetUser.Username, sender.Username, lobby.GameCode);
            await Clients.Caller.SendAsync("LobbyInviteSent", targetUser.Username, false);
        }
    }

    private async Task NotifyGameFinished(Lobby lobby)
    {
        CancelTurnTimer(lobby.GameCode);
        var game = lobby.Game;
        var winner = game?.GetWinner();

        if (game is not null)
        {
            try
            {
                await PersistMatchAsync(game, winner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist match for lobby {Code}", lobby.GameCode);
            }
        }

        await _hubContext.Clients.Group(lobby.GameCode).SendAsync("GameFinished", winner);

        // Tear down the lobby so it doesn't linger as a zombie.
        var connectionIds = lobby.Players.Keys.ToList();
        foreach (var connId in connectionIds)
        {
            try { await Groups.RemoveFromGroupAsync(connId, lobby.GameCode); } catch { /* best-effort */ }
        }
        _lobbyManager.RemoveLobby(lobby.GameCode);
    }

    private async Task PersistMatchAsync(GameSession game, string? winnerUsername)
    {
        // Only persist participations for registered users (guests have no stable userId).
        var registered = game.Participants
            .Where(kv => !kv.Value.IsGuest && kv.Value.UserId > 0)
            .ToList();

        if (registered.Count == 0) return;

        var match = Match.Create();
        await _matchRepository.AddAsync(match);
        await _unitOfWork.SaveChangesAsync(); // Generate match.Id

        foreach (var (username, info) in registered)
        {
            var participation = match.AddParticipant(info.UserId);
            if (game.Scores.TryGetValue(username, out var score) && score > 0)
                participation.AddPoints(score);
        }

        int? winnerUserId = null;
        if (winnerUsername is not null
            && game.Participants.TryGetValue(winnerUsername, out var winnerInfo)
            && !winnerInfo.IsGuest && winnerInfo.UserId > 0)
        {
            winnerUserId = winnerInfo.UserId;
        }

        match.Finish(winnerUserId);
        _matchRepository.Update(match);
        await _unitOfWork.SaveChangesAsync();
    }

    private LobbyPlayer CreatePlayerFromContext(bool isHost)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var username = Context.User?.Identity?.Name
                    ?? Context.User?.FindFirstValue("username")
                    ?? "Guest_" + Context.ConnectionId[..4];

        var isGuest = string.IsNullOrEmpty(userIdStr) || userIdStr == "0";
        var userId = isGuest ? 0 : int.Parse(userIdStr!);

        return new LobbyPlayer(Context.ConnectionId, userId, username, isGuest, isHost);
    }

    private void StartTurnTimer(Lobby lobby, int seconds)
    {
        var cts = new CancellationTokenSource();
        _turnTimers.AddOrUpdate(lobby.GameCode, _ => cts, (_, old) => { old.Cancel(); return cts; });

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds + 2), cts.Token);
                await HandleTurnTimeout(lobby.GameCode);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Turn timer error for {Lobby}", lobby.GameCode);
            }
        });
    }

    private void CancelTurnTimer(string lobbyCode)
    {
        if (_turnTimers.TryRemove(lobbyCode, out var cts))
        {
            cts.Cancel();
        }
    }

    private async Task HandleTurnTimeout(string lobbyCode)
    {
        var lobby = _lobbyManager.GetLobby(lobbyCode);
        if (lobby?.Game is null || lobby.Game.IsFinished) return;

        var game = lobby.Game;
        var group = _hubContext.Clients.Group(lobby.GameCode);

        if (game.IsWaitingForSecondFlip && game.FirstFlippedCard is not null)
        {
            var firstIdx = game.FirstFlippedCard.Index;
            game.AdvanceTurn();
            await group.SendAsync("CardsHidden", firstIdx, -1);
        }
        else
        {
            game.AdvanceTurn();
        }

        await group.SendAsync("UpdateTurn", game.CurrentPlayer, game.TurnTimeSeconds);
        StartTurnTimer(lobby, game.TurnTimeSeconds);
    }
}
