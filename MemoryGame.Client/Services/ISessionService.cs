using MemoryGame.Client.Models;

namespace MemoryGame.Client.Services;

/// <summary>
/// Manages the current user session and persists tokens.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// The current session, or null if not logged in.
    /// </summary>
    UserSession? Current { get; }

    /// <summary>
    /// Whether the user is currently authenticated.
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// Stores the session data after a successful login.
    /// </summary>
    void StartSession(UserSession session);

    /// <summary>
    /// Clears the session data on logout.
    /// </summary>
    void EndSession();
}
