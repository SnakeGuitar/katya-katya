using KatyaKatya.Models.Lobby;

namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Handles all in-game SignalR events and actions (turns, matching cards, scores).
/// </summary>
public interface IGameService
{
    event Action<List<CardInfoDto>>? GameStarted;
    event Action<string, int>? TurnUpdated;
    event Action<int, string?>? CardShown;
    event Action<int, int>? CardsMatched;
    event Action<int, int>? CardsHidden;
    event Action<string, int>? ScoreUpdated;
    event Action<string>? GameFinished;

    Task StartGameAsync(GameSettingsDto settings);
    Task FlipCardAsync(int cardIndex);
}
