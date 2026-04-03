namespace MemoryGame.Client.Models;

/// <summary>
/// DTO matching the server's profile response.
/// </summary>
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
