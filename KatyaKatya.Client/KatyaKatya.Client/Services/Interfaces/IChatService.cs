namespace KatyaKatya.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Event fired when a chat message is received from the lobby.
    /// Parameters: username, message, isSystemMessage
    /// </summary>
    event Action<string, string, bool>? MessageReceived;

    Task SendChatMessageAsync(string message);
}
