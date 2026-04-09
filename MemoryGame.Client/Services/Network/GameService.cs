using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.Client.Models.Lobby;

namespace MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.Interfaces;

public class GameService : IGameService
{
    private readonly HubService _hubService;

    public event Action<List<CardInfoDto>>? GameStarted;
    public event Action<string, int>? TurnUpdated;
    public event Action<int, string?>? CardShown;
    public event Action<int, int>? CardsMatched;
    public event Action<int, int>? CardsHidden;
    public event Action<string, int>? ScoreUpdated;
    public event Action<string>? GameFinished;

    public GameService(HubService hubService)
    {
        _hubService = hubService;
        _hubService.ConnectionEstablished += AttachHandlers;
    }

    private void AttachHandlers(HubConnection connection)
    {
        connection.On<List<CardInfoDto>>("GameStarted", board => GameStarted?.Invoke(board));
        connection.On<string, int>("UpdateTurn", (user, time) => TurnUpdated?.Invoke(user, time));
        connection.On<int, string?>("ShowCard", (index, image) => CardShown?.Invoke(index, image));
        connection.On<int, int>("SetCardsAsMatched", (i1, i2) => CardsMatched?.Invoke(i1, i2));
        connection.On<int, int>("HideCards", (i1, i2) => CardsHidden?.Invoke(i1, i2));
        connection.On<string, int>("UpdateScore", (user, score) => ScoreUpdated?.Invoke(user, score));
        connection.On<string>("GameFinished", winner => GameFinished?.Invoke(winner));
    }

    public async Task StartGameAsync(GameSettingsDto settings)
    {
        if (_hubService.Connection is not null)
            await _hubService.Connection.InvokeAsync("StartGame", settings);
    }

    public async Task FlipCardAsync(int cardIndex)
    {
        if (_hubService.Connection is not null)
            await _hubService.Connection.InvokeAsync("FlipCard", cardIndex);
    }
}
