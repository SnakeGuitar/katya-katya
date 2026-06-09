using KatyaKatya.Models.Lobby;

namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Handles all pre-game and lobby management SignalR events.
/// </summary>
public interface ILobbyService
{
    event Action<string>? LobbyCreated;
    event Action<List<LobbyPlayerDto>>? PlayerListUpdated;
    event Action<string, bool>? PlayerJoined;
    event Action<string>? PlayerLeft;
    event Action? Kicked;
    event Action<List<LobbySummaryDto>>? PublicLobbiesUpdated;
    event Action<string, string>? LobbyInviteReceived;
    event Action<string, bool>? LobbyInviteSent;
    event Action<string>? ErrorReceived;

    List<LobbyPlayerDto> CurrentPlayers { get; }

    Task CreateLobbyAsync(string gameCode, bool isPublic);
    Task JoinLobbyAsync(string gameCode);
    Task LeaveLobbyAsync();
    Task VoteToKickAsync(string targetUsername);
    Task KickPlayerAsync(string targetUsername);
    Task GetPublicLobbiesAsync();
    Task InviteFriendAsync(int targetUserId);
}
