namespace KatyaKatya.Models.Lobby;

public record LobbySummaryDto(string GameCode, string HostUsername, int CurrentPlayers, int MaxPlayers, bool IsFull);
