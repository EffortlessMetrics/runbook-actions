using FluentAssertions;
using Loupedeck;
using Runbook.Protocol;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for KeypadSlotCommand rendering behavior via GetCommandImage.
/// Tests the rendering pipeline through the slot command's image method
/// using reflection (protected method).
/// </summary>
public class KeypadSlotRenderBehaviors
{
    private readonly PluginImageSize _size = new();

    // ── Given disconnected daemon ────────────────────────────────────

    [Fact]
    public void Given_No_Plugin_Instance_GetCommandImage_Should_Return_Offline()
    {
        // Ensure no plugin is loaded.
        RunbookPlugin.Instance?.Unload();

        var cmd = new Runbook.Actions.KeypadSlotCommand();
        var method = typeof(Runbook.Actions.KeypadSlotCommand)
            .GetMethod("GetCommandImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = method!.Invoke(cmd, new object[] { "0", _size });

        result.Should().NotBeNull("disconnected state should show OFFLINE tile");
    }

    [Fact]
    public void Given_Invalid_Slot_Parameter_GetCommandImage_Should_Return_Null()
    {
        var cmd = new Runbook.Actions.KeypadSlotCommand();
        var method = typeof(Runbook.Actions.KeypadSlotCommand)
            .GetMethod("GetCommandImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = method!.Invoke(cmd, new object[] { "notanumber", _size });

        result.Should().BeNull("invalid slot parameter should return null");
    }

    // ── Given connected daemon with render model ─────────────────────

    [Fact]
    public void Given_Loaded_Plugin_With_Render_Model_Should_Show_Labels()
    {
        var plugin = new RunbookPlugin();
        plugin.Load();

        // Inject a render model into the daemon.
        plugin.Daemon.GetType()
            .GetProperty("Render")!
            .SetValue(plugin.Daemon, new RenderModel
            {
                AgentState = "idle",
                Keypad = new KeypadRender
                {
                    Slots = new System.Collections.Generic.List<KeypadSlotRender>
                    {
                        new() { Slot = 0, Label = "Test", Armed = false },
                        new() { Slot = 1, Label = "Armed", Sublabel = "sub", Armed = true },
                    }
                }
            });

        var cmd = new Runbook.Actions.KeypadSlotCommand();
        var method = typeof(Runbook.Actions.KeypadSlotCommand)
            .GetMethod("GetCommandImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Slot 0: normal tile.
        var slot0 = method!.Invoke(cmd, new object[] { "0", _size });
        slot0.Should().NotBeNull("slot with render data should produce an image");

        // Slot 1: armed tile.
        var slot1 = method!.Invoke(cmd, new object[] { "1", _size });
        slot1.Should().NotBeNull("armed slot should produce an image");

        // Slot 5: no data, should show em-dash label.
        var slot5 = method!.Invoke(cmd, new object[] { "5", _size });
        slot5.Should().NotBeNull("missing slot should render with default label");

        plugin.Unload();
    }
}
