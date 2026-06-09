using KatyaKatya.Models;

namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Manages the current user session and persists tokens.
/// </summary>
public interface ISessionService
{
    UserSession? Current { get; }
    bool IsLoggedIn { get; }
    void StartSession(UserSession session);
    void EndSession();
}
