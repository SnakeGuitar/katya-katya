using Microsoft.AspNetCore.SignalR.Client;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.Network;

/// <summary>
/// Manages the SignalR hub connection lifecycle.
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

    public event Action<HubConnection>? ConnectionEstablished;
    public HubConnection? Connection => _connection;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task ConnectAsync()
    {
        Task? taskToWait = null;

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection?.State == HubConnectionState.Connected)
                return;

            if (_connectTask != null)
                taskToWait = _connectTask;
            else
            {
                _connectTask = ConnectInternalAsync();
                taskToWait = _connectTask;
            }
        }
        finally
        {
            _connectionLock.Release();
        }

        if (taskToWait != null)
            await taskToWait;
    }

    private async Task ConnectInternalAsync()
    {
        try
        {
            if (_connection is not null)
            {
                try { await _connection.DisposeAsync(); } catch { }
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HubService] Connection failed: {ex.Message}");
            throw;
        }
        finally
        {
            await _connectionLock.WaitAsync();
            _connectTask = null;
            _connectionLock.Release();
        }
    }

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

    public async ValueTask DisposeAsync() => await DisconnectAsync();

    public void Dispose()
    {
        if (_connection is not null)
            DisconnectAsync().GetAwaiter().GetResult();
    }
}
