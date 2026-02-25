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
        return base.OnLoad();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!int.TryParse(actionParameter, out var slot) || slot < 0 || slot > 8)
            return;

        _ = RunbookPlugin.Instance?.Daemon.SendKeypadPressAsync(slot);
    }

    protected override BitmapImage? GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        if (!int.TryParse(actionParameter, out var slot) || slot < 0 || slot > 8)
            return null;

        var daemon = RunbookPlugin.Instance?.Daemon;

        if (daemon is null || daemon.State != Daemon.ConnectionState.Connected)
            return Render.KeyRenderer.RenderOffline(imageSize);

        var render = daemon.Render;
        var slotRender = render?.Keypad?.Slots?.FirstOrDefault(s => s.Slot == slot);

        var label = slotRender?.Label ?? "\u2014";
        var sub = slotRender?.Sublabel;
        var armed = slotRender?.Armed ?? false;

        return Render.KeyRenderer.RenderSlot(imageSize, slot, label, sub, armed);
    }
}
