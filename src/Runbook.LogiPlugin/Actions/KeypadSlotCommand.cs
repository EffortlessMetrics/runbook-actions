using System;
using System.Linq;

using Loupedeck;

namespace Runbook.Actions;

/// <summary>
/// One dynamic command, parameterized by slot index (0..8).
/// Assign this action to all 9 LCD keys with parameters "0".."8".
/// </summary>
public sealed class KeypadSlotCommand : PluginDynamicCommand
{
    public KeypadSlotCommand()
        : base(displayName: "Runbook Slot", description: "Arms a Runbook prompt", groupName: "Runbook")
    {
    }

    protected override bool OnLoad()
    {
        // Subscribe to daemon render updates so we can invalidate images.
        if (RunbookPlugin.Instance?.Daemon is { } daemon)
        {
            daemon.RenderUpdated += (_, _) => ActionImageChanged();
            daemon.StateChanged += (_, _) => ActionImageChanged();
        }

        return base.OnLoad();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!int.TryParse(actionParameter, out var slot))
            return;

        _ = RunbookPlugin.Instance?.Daemon.SendKeypadPressAsync(slot);
    }

    protected override BitmapImage? GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        if (!int.TryParse(actionParameter, out var slot))
            return null;

        var daemon = RunbookPlugin.Instance?.Daemon;

        // OFFLINE tile when daemon is not connected.
        if (daemon is null || daemon.State != Daemon.ConnectionState.Connected)
        {
            using var offBb = new BitmapBuilder(imageSize);
            offBb.Clear(BitmapColor.Black);
            offBb.DrawText("OFFLINE");
            return offBb.ToImage();
        }

        var render = daemon.Render;
        var slotRender = render?.Keypad?.Slots?.FirstOrDefault(s => s.Slot == slot);

        var label = slotRender?.Label ?? "—";
        var sub = slotRender?.Sublabel;
        var armed = slotRender?.Armed ?? false;

        using var bb = new BitmapBuilder(imageSize);
        bb.Clear(armed ? new BitmapColor(0, 102, 204) : BitmapColor.Black);

        var textColor = armed ? BitmapColor.White : new BitmapColor(200, 200, 200);

        // SDK guidance: do NOT force fontSize — the runtime picks best size per device.
        bb.DrawText(label, color: textColor);

        if (!string.IsNullOrEmpty(sub))
        {
            bb.DrawText(sub!, color: textColor);
        }

        // Border for armed state.
        if (armed)
        {
            bb.DrawRectangle(0, 0, imageSize.Width - 1, imageSize.Height - 1, BitmapColor.White);
            bb.DrawRectangle(1, 1, imageSize.Width - 3, imageSize.Height - 3, BitmapColor.White);
        }

        return bb.ToImage();
    }
}
