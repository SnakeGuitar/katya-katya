namespace KatyaKatya.Models;

public record ProfileResponse(
    int Id,
    string Username,
    string? Name,
    string? LastName,
    string Email,
    bool IsGuest,
    bool VerifiedEmail,
    DateTime RegistrationDate,
    byte[]? Avatar);
