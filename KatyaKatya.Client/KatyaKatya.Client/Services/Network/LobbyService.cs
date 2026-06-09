using Microsoft.AspNetCore.SignalR.Client;
using KatyaKatya.Models.Lobby;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.Network;

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

    public List<LobbyPlayerDto> CurrentPlayers { get; } = new();

    public LobbyService(HubService hubService)
    {
        _hubService = hubService;
        _hubService.ConnectionEstablished += AttachHandlers;

        if (_hubService.Connection is not null)
            AttachHandlers(_hubService.Connection);
    }

    private void AttachHandlers(HubConnection connection)
    {
        connection.On<string>("LobbyCreated", code => LobbyCreated?.Invoke(code));

        connection.On<List<LobbyPlayerDto>>("UpdatePlayerList", players =>
        {
            CurrentPlayers.Clear();
            CurrentPlayers.AddRange(players);
            PlayerListUpdated?.Invoke(players);
        });

        connection.On<string, bool>("PlayerJoined", (user, guest) => PlayerJoined?.Invoke(user, guest));
        connection.On<string>("PlayerLeft", user => PlayerLeft?.Invoke(user));
        connection.On("Kicked", () => Kicked?.Invoke());
        connection.On<List<LobbySummaryDto>>("PublicLobbiesList", lobbies => PublicLobbiesUpdated?.Invoke(lobbies));
        connection.On<string, string>("LobbyInviteReceived", (caller, code) => LobbyInviteReceived?.Invoke(caller, code));
        connection.On<string, bool>("LobbyInviteSent", (target, online) => LobbyInviteSent?.Invoke(target, online));
        connection.On<string>("Error", code => ErrorReceived?.Invoke(code));
    }

    public async Task CreateLobbyAsync(string gameCode, bool isPublic)
        => await GetActiveConnection().InvokeAsync("CreateLobby", gameCode, isPublic);

    public async Task JoinLobbyAsync(string gameCode)
        => await GetActiveConnection().InvokeAsync("JoinLobby", gameCode);

    public async Task LeaveLobbyAsync()
    {
        await GetActiveConnection().InvokeAsync("LeaveLobby");
        CurrentPlayers.Clear();
    }

    public async Task VoteToKickAsync(string targetUsername)
        => await GetActiveConnection().InvokeAsync("VoteToKick", targetUsername);

    public async Task KickPlayerAsync(string targetUsername)
        => await GetActiveConnection().InvokeAsync("KickPlayer", targetUsername);

    public async Task GetPublicLobbiesAsync()
        => await GetActiveConnection().InvokeAsync("GetPublicLobbies");

    public async Task InviteFriendAsync(int targetUserId)
        => await GetActiveConnection().InvokeAsync("InviteFriend", targetUserId);

    private HubConnection GetActiveConnection()
    {
        if (_hubService.Connection is null || _hubService.Connection.State != HubConnectionState.Connected)
            throw new InvalidOperationException("Not connected to game server.");
        return _hubService.Connection;
    }
}
