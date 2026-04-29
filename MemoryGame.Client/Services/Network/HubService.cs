using Microsoft.AspNetCore.SignalR.Client;

namespace MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Manages the SignalR hub connection lifecycle.
/// Connects on login, disconnects on logout.
/// </summary>
public class HubService : IAsyncDisposable, IDisposable
{
    private readonly ISessionService _session;
    private readonly string _hubUrl;
    private HubConnection? _connection;

    public HubService(ISessionService session, string hubUrl)
    {
        _session = session;
        _hubUrl = hubUrl;
    }

    /// <summary>
    /// Event fired after a new hub connection is built but before it starts.
    /// Services should listen to this to attach their .On handlers.
    /// </summary>
    public event Action<HubConnection>? ConnectionEstablished;

    /// <summary>
    /// The underlying SignalR connection. Null until <see cref="ConnectAsync"/> is called.
    /// </summary>
    public HubConnection? Connection => _connection;

    /// <summary>
    /// Whether the hub connection is currently active.
    /// </summary>
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    /// <summary>
    /// Builds and starts the hub connection using the current session's JWT.
    /// If already connected, this is a no-op.
    /// </summary>
    public async Task ConnectAsync()
    {
        // Already connected — nothing to do
        if (_connection is not null && _connection.State == HubConnectionState.Connected)
            return;

        // Connection exists but is in a bad state — tear it down first
        if (_connection is not null)
        {
            try { await _connection.DisposeAsync(); } catch { /* best-effort */ }
            _connection = null;
        }

        if (_session.Current is null)
            throw new InvalidOperationException("Cannot connect to hub without an active session.");

        var token = _session.Current.AccessToken;

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        ConnectionEstablished?.Invoke(_connection);

        await _connection.StartAsync();
    }

    /// <summary>
    /// Stops and disposes the hub connection.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    public void Dispose()
    {
        if (_connection is not null)
            DisconnectAsync().GetAwaiter().GetResult();
    }
}
