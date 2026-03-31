using MemoryGame.Client.Models;

namespace MemoryGame.Client.Services;

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
