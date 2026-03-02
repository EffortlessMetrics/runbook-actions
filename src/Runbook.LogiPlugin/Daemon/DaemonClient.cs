using System;
using System.IO;
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
    Connected,
    ProtocolError
}

/// <summary>
/// WebSocket client to runbookd.
///
/// Default endpoint: ws://127.0.0.1:29381/ws
/// Override via plugin settings (daemon_url) or RUNBOOKD_WS env var.
///
/// Connection lifecycle:
///   Connecting → Connected (hello_ack) → recv loop → Disconnected → backoff → …
///   Connecting → ProtocolError (bad hello_ack) → stop
/// </summary>
public sealed class DaemonClient : IAsyncDisposable
{
    private const int KeepAliveSeconds = 15;

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private ConnectionState _state = ConnectionState.Disconnected;
    private bool _disposed;

    // Adjustment coalescing.
    private AdjustmentCoalescer? _adjustmentCoalescer;

    public DaemonClient()
    {
        _adjustmentCoalescer = new AdjustmentCoalescer(TimeSpan.FromMilliseconds(16),
            (kind, delta) => _ = SendRawAsync(new { type = "adjustment", kind, delta }));
    }

    public event EventHandler? RenderUpdated;
    public event EventHandler<ConnectionState>? StateChanged;

    public Render.RenderModel? Render { get; private set; }

    /// <summary>Daemon URL. Set via ConfigureDaemonUrl before ConnectAsync.</summary>
    public string DaemonUrl { get; set; } = "ws://127.0.0.1:29381/ws";

    /// <summary>Client ID persisted via plugin settings.</summary>
    public string ClientId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Human-readable detail when State == ProtocolError.</summary>
    public string? ProtocolErrorDetail { get; private set; }

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

        // Override from env var if present.
        var envUrl = Environment.GetEnvironmentVariable("RUNBOOKD_WS");
        if (!string.IsNullOrEmpty(envUrl))
            DaemonUrl = envUrl;

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

                await _ws.ConnectAsync(new Uri(DaemonUrl), _cts.Token);

                // Hello handshake.
                await SendRawAsync(new
                {
                    type = "hello",
                    client = "logi",
                    protocol = 1,
                    version = "0.1.0",
                    client_id = ClientId
                });

                // Wait for hello_ack (timeout 5s).
                var ackJson = await ReceiveOneMessageAsync(TimeSpan.FromSeconds(5));
                if (ackJson is not null)
                {
                    using var doc = JsonDocument.Parse(ackJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("type", out var t) && t.GetString() == "hello_ack")
                    {
                        if (root.TryGetProperty("protocol", out var p) && p.GetInt32() != 1)
                        {
                            ProtocolErrorDetail = $"plugin=1 daemon={p.GetInt32()}";
                            State = ConnectionState.ProtocolError;
                            return; // Stop reconnecting.
                        }
                        // Handshake OK.
                    }
                }

                State = ConnectionState.Connected;
                backoff = TimeSpan.FromMilliseconds(500);

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
                try { _ws?.Dispose(); }
                catch { }
                _ws = null;
                if (!_disposed && _state != ConnectionState.ProtocolError)
                    State = ConnectionState.Disconnected;
            }

            if (_disposed || _state == ConnectionState.ProtocolError) break;

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

    // ── Public send methods ──────────────────────────────────────────

    public Task SendKeypadPressAsync(int slot)
        => SendRawAsync(new { type = "keypad_press", slot });

    public Task SendDialpadButtonPressAsync(string button)
        => SendRawAsync(new { type = "dialpad_button_press", button });

    public Task SendPageAsync(string direction)
        => SendRawAsync(new { type = "page", direction });

    /// <summary>Coalesced: delta is accumulated and flushed on the 16ms timer.</summary>
    public void EnqueueAdjustment(string kind, int delta)
        => _adjustmentCoalescer?.Enqueue(kind, delta);

    /// <summary>Non-coalesced adjustment send (legacy / direct).</summary>
    public Task SendAdjustmentAsync(string kind, int delta)
        => SendRawAsync(new { type = "adjustment", kind, delta });

    // ── Internal send/receive ────────────────────────────────────────

    private async Task SendRawAsync(object message)
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

    /// <summary>Receive one complete WS message, handling multi-frame.</summary>
    private async Task<string?> ReceiveOneMessageAsync(TimeSpan? timeout = null)
    {
        if (_ws is null) return null;

        using var cts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(_cts?.Token ?? CancellationToken.None)
            : null;
        if (cts is not null) cts.CancelAfter(timeout!.Value);
        var token = cts?.Token ?? _cts?.Token ?? CancellationToken.None;

        var ms = new MemoryStream();
        var buffer = new byte[4096];

        while (true)
        {
            var result = await _ws.ReceiveAsync(buffer, token);
            if (result.MessageType == WebSocketMessageType.Close)
                return null;

            ms.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
                break;
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private async Task ReceiveLoopAsync()
    {
        if (_ws is null) return;

        while (_ws.State == WebSocketState.Open && _cts is { IsCancellationRequested: false })
        {
            var json = await ReceiveOneMessageAsync();
            if (json is null) break;

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

    // ── Lifecycle ────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        _adjustmentCoalescer?.Dispose();
        try { _cts?.Cancel(); }
        catch { }

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
        _adjustmentCoalescer = null;
        State = ConnectionState.Disconnected;
    }
}
