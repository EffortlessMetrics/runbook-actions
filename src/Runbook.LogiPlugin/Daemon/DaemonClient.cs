using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Runbook.Daemon;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected
}

/// <summary>
/// WebSocket client to runbookd.
///
/// Default endpoint: ws://127.0.0.1:29381/ws
/// Override via RUNBOOKD_WS environment variable.
///
/// Connection lifecycle:
///   Connecting → Connected → (recv loop) → Disconnected → backoff → Connecting …
/// </summary>
public sealed class DaemonClient : IAsyncDisposable
{
    private const int KeepAliveSeconds = 15;

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private ConnectionState _state = ConnectionState.Disconnected;
    private bool _disposed;

    public event EventHandler? RenderUpdated;
    public event EventHandler<ConnectionState>? StateChanged;

    public Render.RenderModel? Render { get; private set; }

    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }
    }

    public Task ConnectAsync()
    {
        if (_disposed) return Task.CompletedTask;

        _cts = new CancellationTokenSource();
        _ = Task.Run(MaintainConnectionAsync);
        return Task.CompletedTask;
    }

    private async Task MaintainConnectionAsync()
    {
        var backoff = TimeSpan.FromMilliseconds(500);
        var maxBackoff = TimeSpan.FromSeconds(5);

        while (_cts is { IsCancellationRequested: false })
        {
            try
            {
                State = ConnectionState.Connecting;
                _ws = new ClientWebSocket();
                _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(KeepAliveSeconds);

                var url = Environment.GetEnvironmentVariable("RUNBOOKD_WS")
                          ?? "ws://127.0.0.1:29381/ws";

                await _ws.ConnectAsync(new Uri(url), _cts.Token);

                State = ConnectionState.Connected;
                backoff = TimeSpan.FromMilliseconds(500); // Reset on success.

                // Hello handshake.
                await SendAsync(new
                {
                    type = "hello",
                    client = "logi",
                    protocol = 1,
                    version = "0.1.0"
                });

                await ReceiveLoopAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Fall through to reconnect.
            }
            finally
            {
                try { _ws?.Dispose(); } catch { }
                _ws = null;
                if (!_disposed) State = ConnectionState.Disconnected;
            }

            if (_disposed) break;

            try
            {
                await Task.Delay(backoff, _cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            backoff = TimeSpan.FromTicks(Math.Min(maxBackoff.Ticks, backoff.Ticks * 2));
        }
    }

    public Task SendKeypadPressAsync(int slot)
        => SendAsync(new { type = "keypad_press", slot });

    public Task SendDialpadButtonPressAsync(string button)
        => SendAsync(new { type = "dialpad_button_press", button });

    public Task SendAdjustmentAsync(string kind, int delta)
        => SendAsync(new { type = "adjustment", kind, delta });

    private async Task SendAsync(object message)
    {
        if (_ws is null || _ws.State != WebSocketState.Open)
            return;

        try
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true,
                _cts?.Token ?? CancellationToken.None);
        }
        catch
        {
            // Best effort — send failures are non-fatal.
        }
    }

    private async Task ReceiveLoopAsync()
    {
        if (_ws is null) return;

        var buffer = new byte[64 * 1024];

        while (_ws.State == WebSocketState.Open && _cts is { IsCancellationRequested: false })
        {
            var result = await _ws.ReceiveAsync(buffer, _cts.Token);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                    continue;

                if (typeProp.GetString() == "render")
                {
                    Render = JsonSerializer.Deserialize<Render.RenderModel>(json);
                    RenderUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                // Ignore malformed messages.
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try { _cts?.Cancel(); } catch { }

        if (_ws is not null)
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown",
                    CancellationToken.None);
            }
            catch { }
            _ws.Dispose();
        }

        _cts?.Dispose();
        _ws = null;
        _cts = null;
        State = ConnectionState.Disconnected;
    }
}
