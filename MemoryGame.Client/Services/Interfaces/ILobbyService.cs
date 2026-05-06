using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemoryGame.Client.Models.Lobby;

namespace MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Handles all pre-game and lobby management SignalR events (joining, kicks, friends, listing).
/// </summary>
public interface ILobbyService
{
    // Hub events
    event Action<string>? LobbyCreated;
    event Action<List<LobbyPlayerDto>>? PlayerListUpdated;
    event Action<string, bool>? PlayerJoined;
    event Action<string>? PlayerLeft;
    event Action? Kicked;
    
    event Action<List<LobbySummaryDto>>? PublicLobbiesUpdated;
    event Action<string, string>? LobbyInviteReceived;
    event Action<string, bool>? LobbyInviteSent;
    
    event Action<string>? ErrorReceived;
    
    /// <summary>The current list of players in the joined lobby.</summary>
    List<LobbyPlayerDto> CurrentPlayers { get; }

    // Hub methods
    Task CreateLobbyAsync(string gameCode, bool isPublic);
    Task JoinLobbyAsync(string gameCode);
    Task LeaveLobbyAsync();
    
    Task VoteToKickAsync(string targetUsername);
    Task KickPlayerAsync(string targetUsername);
    Task GetPublicLobbiesAsync();
    Task InviteFriendAsync(int targetUserId);
}
