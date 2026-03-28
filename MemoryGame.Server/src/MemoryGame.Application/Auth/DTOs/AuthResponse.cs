namespace MemoryGame.Application.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);
