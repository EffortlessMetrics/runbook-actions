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
        // Optional: subscribe to daemon render updates so we can invalidate images.
        if (RunbookPlugin.Instance?.Daemon is { } daemon)
        {
            daemon.RenderUpdated += (_, _) =>
            {
                // Invalidate all slot images. (Per-action invalidation is better if supported.)
                this.ActionImageChanged();
            };
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

        var render = RunbookPlugin.Instance?.Daemon.Render;
        var slotRender = render?.Keypad?.Slots?.FirstOrDefault(s => s.Slot == slot);

        var label = slotRender?.Label ?? "—";
        var sub = slotRender?.Sublabel;
        var armed = slotRender?.Armed ?? false;

        // The SDK typically provides a BitmapBuilder helper.
        // Replace this with the SDK's actual drawing API.
        var bb = new BitmapBuilder(imageSize);
        bb.Clear();
        bb.DrawText(label, x: 0, y: 0, width: imageSize.Width, height: imageSize.Height / 2, fontSize: 16);
        if (!string.IsNullOrEmpty(sub))
        {
            bb.DrawText(sub!, x: 0, y: imageSize.Height / 2, width: imageSize.Width, height: imageSize.Height / 2, fontSize: 12);
        }
        if (armed)
        {
            bb.DrawRectangle(0, 0, imageSize.Width - 1, imageSize.Height - 1);
        }

        return bb.ToImage();
    }
}
