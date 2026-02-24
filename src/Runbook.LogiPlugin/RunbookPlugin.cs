using System;

// These namespaces come from the Logi Actions / Loupedeck SDK.
// The exact assembly references depend on how you install the SDK.
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
        Daemon.StateChanged += (_, state) =>
        {
            var status = state switch
            {
                Runbook.Daemon.ConnectionState.Connected => PluginStatus.Normal,
                Runbook.Daemon.ConnectionState.Connecting => PluginStatus.Warning,
                _ => PluginStatus.Error
            };
            var message = state switch
            {
                Runbook.Daemon.ConnectionState.Connected => "Connected to runbookd",
                Runbook.Daemon.ConnectionState.Connecting => "Connecting to runbookd...",
                _ => "Daemon offline"
            };

            this.OnStatusChanged(status, message);
        };

        Daemon.RenderUpdated += (_, _) =>
        {
            // Invalidate all keypad slot images when the render model changes.
            for (var i = 0; i < 9; i++)
            {
                this.ActionImageChanged("Runbook Slot", i.ToString());
            }
        };

        _ = Daemon.ConnectAsync();
    }

    public override void Unload()
    {
        _ = Daemon.DisposeAsync();
        Instance = null;
    }
}
