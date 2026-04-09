using System;
using System.Threading.Tasks;

namespace MemoryGame.Client.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Event fired when a chat message is received from the lobby.
    /// Parameters: username, message, isSystemMessage
    /// </summary>
    event Action<string, string, bool>? MessageReceived;

    /// <summary>
    /// Sends a chat message to the current lobby.
    /// Does nothing if the current user is a guest.
    /// </summary>
    Task SendChatMessageAsync(string message);
}
