using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemoryGame.Client.Models.Lobby;

namespace MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Handles all in-game SignalR events and actions (turns, matching cards, scores).
/// </summary>
public interface IGameService
{
    // Hub events
    event Action<List<CardInfoDto>>? GameStarted;
    event Action<string, int>? TurnUpdated;
    event Action<int, string?>? CardShown;
    event Action<int, int>? CardsMatched;
    event Action<int, int>? CardsHidden;
    event Action<string, int>? ScoreUpdated;
    event Action<string>? GameFinished;

    // Hub methods
    Task StartGameAsync(GameSettingsDto settings);
    Task FlipCardAsync(int cardIndex);
}
