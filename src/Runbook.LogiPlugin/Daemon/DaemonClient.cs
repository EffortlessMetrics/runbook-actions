using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Runbook.Daemon;

/// <summary>
/// WebSocket client to runbookd.
///
/// Default endpoint: ws://127.0.0.1:29381/ws
/// Override via RUNBOOKD_WS environment variable.
/// </summary>
public sealed class DaemonClient : IAsyncDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public event EventHandler? RenderUpdated;

    public Render.RenderModel? Render { get; private set; }

    public async Task ConnectAsync()
    {
        if (_ws is not null)
            return;

        _cts = new CancellationTokenSource();
        _ws = new ClientWebSocket();

        var url = Environment.GetEnvironmentVariable("RUNBOOKD_WS")
                  ?? "ws://127.0.0.1:29381/ws";

        await _ws.ConnectAsync(new Uri(url), _cts.Token);

        // Hello handshake (best-effort; daemon tolerates duplicates).
        await SendAsync(new
        {
            type = "hello",
            client = "logi",
            protocol = 1,
            version = "0.1.0"
        });

        _ = Task.Run(ReceiveLoopAsync);
    }

    public async Task SendKeypadPressAsync(int slot)
        => await SendAsync(new { type = "keypad_press", slot });

    public async Task SendDialpadButtonPressAsync(string button)
        => await SendAsync(new { type = "dialpad_button_press", button });

    public async Task SendAdjustmentAsync(string kind, int delta)
        => await SendAsync(new { type = "adjustment", kind, delta });

    private async Task SendAsync(object message)
    {
        if (_ws is null)
            return;

        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, _cts!.Token);
    }

    private async Task ReceiveLoopAsync()
    {
        if (_ws is null)
            return;

        var buffer = new byte[64 * 1024];

        while (_ws.State == WebSocketState.Open)
        {
            var result = await _ws.ReceiveAsync(buffer, _cts!.Token);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                    continue;

                var type = typeProp.GetString();
                if (type == "render")
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
        try
        {
            _cts?.Cancel();
        }
        catch { }

        if (_ws is not null)
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", CancellationToken.None);
            }
            catch { }
            _ws.Dispose();
        }

        _cts?.Dispose();

        _ws = null;
        _cts = null;
    }
}
