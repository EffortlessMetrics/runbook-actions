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

        // Plugin metadata (shown in Logi Options+).
        // TODO: set icon/name/description via the SDK's Info model.

        Daemon = new Daemon.DaemonClient();
        Daemon.RenderUpdated += (_, _) =>
        {
            // Invalidate all action images.
            // The SDK provides a way to notify that images changed.
            // TODO: call ActionImageChanged("0".."8") for keypad slots.
        };

        _ = Daemon.ConnectAsync();
    }

    public override void Unload()
    {
        _ = Daemon.DisposeAsync();
        Instance = null;
    }
}
