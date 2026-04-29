using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.Client.Models.Lobby;

namespace MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.Interfaces;

public class LobbyService : ILobbyService
{
    private readonly HubService _hubService;

    public event Action<string>? LobbyCreated;
    public event Action<List<LobbyPlayerDto>>? PlayerListUpdated;
    public event Action<string, bool>? PlayerJoined;
    public event Action<string>? PlayerLeft;
    public event Action? Kicked;
    
    public event Action<List<LobbySummaryDto>>? PublicLobbiesUpdated;
    public event Action<string, string>? LobbyInviteReceived;
    public event Action<string, bool>? LobbyInviteSent;
    
    public event Action<string>? ErrorReceived;

    public LobbyService(HubService hubService)
    {
        _hubService = hubService;
        _hubService.ConnectionEstablished += AttachHandlers;

        if (_hubService.Connection is not null)
        {
            AttachHandlers(_hubService.Connection);
        }
    }

    private void AttachHandlers(HubConnection connection)
    {
        connection.On<string>("LobbyCreated", code => LobbyCreated?.Invoke(code));
        
        connection.On<List<LobbyPlayerDto>>("UpdatePlayerList", players => PlayerListUpdated?.Invoke(players));

        connection.On<string, bool>("PlayerJoined", (user, guest) => PlayerJoined?.Invoke(user, guest));
        connection.On<string>("PlayerLeft", user => PlayerLeft?.Invoke(user));
        connection.On("Kicked", () => Kicked?.Invoke());

        connection.On<List<LobbySummaryDto>>("PublicLobbiesList", lobbies => PublicLobbiesUpdated?.Invoke(lobbies));
        connection.On<string, string>("LobbyInviteReceived", (caller, code) => LobbyInviteReceived?.Invoke(caller, code));
        connection.On<string, bool>("LobbyInviteSent", (target, online) => LobbyInviteSent?.Invoke(target, online));

        connection.On<string>("Error", code => ErrorReceived?.Invoke(code));
    }

    public async Task CreateLobbyAsync(string gameCode, bool isPublic)
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("CreateLobby", gameCode, isPublic);
    }

    public async Task JoinLobbyAsync(string gameCode)
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("JoinLobby", gameCode);
    }

    public async Task LeaveLobbyAsync()
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("LeaveLobby");
    }

    public async Task VoteToKickAsync(string targetUsername)
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("VoteToKick", targetUsername);
    }

    public async Task GetPublicLobbiesAsync()
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("GetPublicLobbies");
    }

    public async Task InviteFriendAsync(int targetUserId)
    {
        var connection = GetActiveConnection();
        await connection.InvokeAsync("InviteFriend", targetUserId);
    }

    private HubConnection GetActiveConnection()
    {
        if (_hubService.Connection is null || _hubService.Connection.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to game server.");
        }
        return _hubService.Connection;
    }
}
