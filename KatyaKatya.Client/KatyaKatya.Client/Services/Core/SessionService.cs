using KatyaKatya.Models;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.Core;

/// <summary>
/// In-memory session holder. Registered as singleton.
/// </summary>
public class SessionService : ISessionService
{
    public UserSession? Current { get; private set; }
    public bool IsLoggedIn => Current is not null;
    public void StartSession(UserSession session) => Current = session;
    public void EndSession() => Current = null;
}
