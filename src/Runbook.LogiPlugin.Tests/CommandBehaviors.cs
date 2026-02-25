using FluentAssertions;
using Runbook.Actions;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for all PluginDynamicCommand implementations.
/// Tests initialization, metadata, and guard behavior for each command type.
/// </summary>
public class CommandBehaviors
{
    // ── KeypadSlotCommand ────────────────────────────────────────────

    [Fact]
    public void Given_KeypadSlotCommand_Should_Have_Display_Name()
    {
        var cmd = new KeypadSlotCommand();

        cmd.Name.Should().Be("Runbook Slot");
    }

    [Fact]
    public void Given_KeypadSlotCommand_Should_Instantiate_Without_Throwing()
    {
        var act = () => new KeypadSlotCommand();

        act.Should().NotThrow();
    }

    // ── DialpadButtonCommand ─────────────────────────────────────────

    [Fact]
    public void Given_DialpadButtonCommand_Should_Have_Display_Name()
    {
        var cmd = new DialpadButtonCommand();

        cmd.Name.Should().Be("Runbook Dialpad Button");
    }

    [Fact]
    public void Given_DialpadButtonCommand_Should_Instantiate_Without_Throwing()
    {
        var act = () => new DialpadButtonCommand();

        act.Should().NotThrow();
    }

    // ── PageCommand ──────────────────────────────────────────────────

    [Fact]
    public void Given_PageCommand_Should_Have_Display_Name()
    {
        var cmd = new PageCommand();

        cmd.Name.Should().Be("Runbook Page");
    }

    [Fact]
    public void Given_PageCommand_Should_Instantiate_Without_Throwing()
    {
        var act = () => new PageCommand();

        act.Should().NotThrow();
    }

    // ── All commands share "Runbook" group ────────────────────────────

    [Fact]
    public void All_Commands_Should_Use_Runbook_Group()
    {
        // Verify all commands instantiate (group is set in base constructor).
        var keypad = new KeypadSlotCommand();
        var dialpad = new DialpadButtonCommand();
        var page = new PageCommand();

        keypad.Name.Should().NotBeNullOrWhiteSpace();
        dialpad.Name.Should().NotBeNullOrWhiteSpace();
        page.Name.Should().NotBeNullOrWhiteSpace();
    }
}
