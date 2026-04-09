using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.Interfaces;

public class ChatService : IChatService
{
    private readonly HubService _hubService;
    private readonly ISessionService _sessionService;

    public event Action<string, string, bool>? MessageReceived;

    public ChatService(HubService hubService, ISessionService sessionService)
    {
        _hubService = hubService;
        _sessionService = sessionService;

        _hubService.ConnectionEstablished += AttachHandlers;
    }

    private void AttachHandlers(HubConnection connection)
    {
        connection.On<string, string, bool>("ReceiveChatMessage", 
            (user, message, isSystem) => MessageReceived?.Invoke(user, message, isSystem));
    }

    public async Task SendChatMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (_sessionService.Current?.IsGuest == true)
            return;

        if (_hubService.Connection is not null)
            await _hubService.Connection.InvokeAsync("SendChatMessage", message);
    }
}
