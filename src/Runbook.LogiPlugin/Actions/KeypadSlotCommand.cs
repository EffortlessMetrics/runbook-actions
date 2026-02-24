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

        using var bb = new BitmapBuilder(imageSize);
        bb.Clear();

        // Draw background if armed.
        if (armed)
        {
            var armedColor = new BitmapColor(0, 102, 204); // A nice blue
            bb.FillRectangle(0, 0, imageSize.Width, imageSize.Height, armedColor);
        }

        // Labels.
        var textColor = armed ? BitmapColor.White : new BitmapColor(200, 200, 200);
        
        bb.DrawText(label, x: 5, y: 5, width: imageSize.Width - 10, height: imageSize.Height / 2, 
                    color: textColor, fontSize: 18);

        if (!string.IsNullOrEmpty(sub))
        {
            bb.DrawText(sub!, x: 5, y: imageSize.Height / 2, width: imageSize.Width - 10, height: imageSize.Height / 2 - 5,
                        color: textColor, fontSize: 13);
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
