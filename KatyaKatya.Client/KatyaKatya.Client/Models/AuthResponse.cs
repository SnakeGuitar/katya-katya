namespace KatyaKatya.Models;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    AuthUserDto User);

public record AuthUserDto(
    int Id,
    string Username,
    string Email,
    bool IsGuest,
    bool VerifiedEmail);
