using System;

// These namespaces come from the Logi Actions / Loupedeck SDK.
using Loupedeck;

namespace Runbook;

/// <summary>
/// Logi Actions SDK plugin.
///
/// Responsibilities:
/// - Hardware I/O (button presses, dial/roller adjustments)
/// - Rendering (LCD key images)
/// - Bridge to runbookd over localhost
///
/// Non-responsibilities:
/// - State machine (owned by runbookd)
/// - Claude Code lifecycle inference (owned by Claude hooks + runbookd)
/// </summary>
public sealed class RunbookPlugin : Plugin
{
    internal static RunbookPlugin? Instance { get; private set; }

    internal Daemon.DaemonClient Daemon { get; private set; } = null!;

    public override void Load()
    {
        Instance = this;

        Daemon = new Daemon.DaemonClient();

        // Plugin settings → daemon config.
        if (TryGetPluginSetting("daemon_url", out var url) && !string.IsNullOrEmpty(url))
            Daemon.DaemonUrl = url!;

        if (TryGetPluginSetting("client_id", out var cid) && !string.IsNullOrEmpty(cid))
        {
            Daemon.ClientId = cid!;
        }
        else
        {
            // Persist a generated client_id on first run.
            SetPluginSetting("client_id", Daemon.ClientId);
        }

        // Connection health → Plugin Status badge in Logi Options+.
        Daemon.StateChanged += OnDaemonStateChanged;

        // Daemon render model changed → redraw LCD keys.
        Daemon.RenderUpdated += (_, _) => InvalidateAllSlots();

        _ = Daemon.ConnectAsync();
    }

    public override void Unload()
    {
        if (Daemon is not null)
        {
            Daemon.StateChanged -= OnDaemonStateChanged;
        }
        _ = Daemon?.DisposeAsync();
        Instance = null;
    }

    private void OnDaemonStateChanged(object? sender, Runbook.Daemon.ConnectionState state)
    {
        var status = state switch
        {
            Runbook.Daemon.ConnectionState.Connected => PluginStatus.Normal,
            Runbook.Daemon.ConnectionState.ProtocolError => PluginStatus.Error,
            _ => PluginStatus.Warning
        };
        var message = state switch
        {
            Runbook.Daemon.ConnectionState.Connected => "Connected to runbookd",
            Runbook.Daemon.ConnectionState.Connecting => "Connecting to runbookd\u2026",
            Runbook.Daemon.ConnectionState.ProtocolError => "Protocol mismatch \u2014 update plugin or daemon",
            _ => "Daemon offline"
        };

        OnPluginStatusChanged(status, message, "https://github.com/runbook-rs");

        // Redraw LCD keys on connect/disconnect so OFFLINE tile appears/disappears.
        InvalidateAllSlots();
    }

    private void InvalidateAllSlots()
    {
        ActionImageChanged();
    }
}
