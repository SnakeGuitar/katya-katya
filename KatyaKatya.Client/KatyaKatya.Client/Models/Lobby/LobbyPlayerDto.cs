namespace KatyaKatya.Models.Lobby;

public record LobbyPlayerDto(int UserId, string Username, bool IsGuest, bool IsHost);
