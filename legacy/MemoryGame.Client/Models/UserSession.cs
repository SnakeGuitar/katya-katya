namespace MemoryGame.Client.Models;

/// <summary>
/// Holds the authenticated user's session data after login.
/// </summary>
public class UserSession
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsGuest { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
