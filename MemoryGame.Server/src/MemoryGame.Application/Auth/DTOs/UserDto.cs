namespace MemoryGame.Application.Auth.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    bool IsGuest,
    bool VerifiedEmail
    );
