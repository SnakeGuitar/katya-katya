using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.Client.Services.Interfaces;

namespace MemoryGame.Client.Services.Network;

/// <summary>
/// Manages the SignalR hub connection lifecycle.
/// Connects on login, disconnects on logout.
/// </summary>
public class HubService : IAsyncDisposable, IDisposable
{
    private readonly ISessionService _session;
    private readonly string _hubUrl;
    private HubConnection? _connection;
    private Task? _connectTask;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

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
    /// If already connected, returns immediately.
    /// If a connection is in progress, waits for it to complete.
    /// </summary>
    public async Task ConnectAsync()
    {
        Task? taskToWait = null;

        await _connectionLock.WaitAsync();
        try
        {
            // If already connected, we're done.
            if (_connection?.State == HubConnectionState.Connected)
                return;

            // If a connection attempt is already in progress, capture the task and wait outside the lock.
            if (_connectTask != null)
            {
                taskToWait = _connectTask;
            }
            else
            {
                // Start a new connection attempt
                _connectTask = ConnectInternalAsync();
                taskToWait = _connectTask;
            }
        }
        finally
        {
            _connectionLock.Release();
        }

        if (taskToWait != null)
        {
            await taskToWait;
        }
    }

    private async Task ConnectInternalAsync()
    {
        try
        {
            if (_connection is not null)
            {
                try { await _connection.DisposeAsync(); } catch { /* best-effort */ }
                _connection = null;
            }

            if (_session.Current is null)
            {
                throw new InvalidOperationException("Cannot connect to hub without an active session.");
            }

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HubService] Connection failed: {ex.Message}");
            throw;
        }
        finally
        {
            // Reset the connect task so the next call can try again if it failed
            await _connectionLock.WaitAsync();
            _connectTask = null;
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Stops and disposes the hub connection.
    /// </summary>
    public async Task DisconnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is not null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
            _connectTask = null;
        }
        finally
        {
            _connectionLock.Release();
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
