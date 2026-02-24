using System;

using Loupedeck;

namespace Runbook.Actions;

/// <summary>
/// Dialpad buttons (Ctrl+C, /export, Esc, Enter).
/// Assign this action to each dialpad button with parameter:
/// - "ctrl_c"
/// - "export"
/// - "esc"
/// - "enter"
/// </summary>
public sealed class DialpadButtonCommand : PluginDynamicCommand
{
    public DialpadButtonCommand()
        : base(displayName: "Runbook Dialpad Button", description: "Runbook dialpad control", groupName: "Runbook")
    {
    }

    protected override void RunCommand(string actionParameter)
    {
        if (string.IsNullOrWhiteSpace(actionParameter))
            return;

        // Forward to daemon. The daemon decides whether Enter dispatches an armed prompt
        // or behaves like a plain Enter keystroke.
        _ = RunbookPlugin.Instance?.Daemon.SendDialpadButtonPressAsync(actionParameter);
    }
}
