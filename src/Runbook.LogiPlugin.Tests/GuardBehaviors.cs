using System;
using System.Reflection;
using FluentAssertions;
using Runbook.Actions;
using Runbook.Adjustments;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for guard behavior in all command/adjustment handlers.
///
/// Every RunCommand / ApplyAdjustment must:
///   - Ignore null / empty / whitespace parameters
///   - Not throw when Instance is null (plugin not loaded)
/// </summary>
public class GuardBehaviors
{
    // ── KeypadSlotCommand guards ─────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanumber")]
    [InlineData("-1")]
    public void KeypadSlot_RunCommand_Should_Ignore_Bad_Parameter(string? param)
    {
        var cmd = new KeypadSlotCommand();
        var method = typeof(KeypadSlotCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object?[] { param ?? string.Empty });

        act.Should().NotThrow("bad parameters must be silently ignored");
    }

    [Fact]
    public void KeypadSlot_RunCommand_With_No_Plugin_Should_Not_Throw()
    {
        // Ensure Instance is null.
        Runbook.RunbookPlugin.Instance?.Unload();

        var cmd = new KeypadSlotCommand();
        var method = typeof(KeypadSlotCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object[] { "0" });

        act.Should().NotThrow("no plugin instance → null-conditional safe");
    }

    // ── DialpadButtonCommand guards ──────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DialpadButton_RunCommand_Should_Ignore_Bad_Parameter(string? param)
    {
        var cmd = new DialpadButtonCommand();
        var method = typeof(DialpadButtonCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object?[] { param ?? string.Empty });

        act.Should().NotThrow();
    }

    [Fact]
    public void DialpadButton_RunCommand_With_No_Plugin_Should_Not_Throw()
    {
        Runbook.RunbookPlugin.Instance?.Unload();

        var cmd = new DialpadButtonCommand();
        var method = typeof(DialpadButtonCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object[] { "enter" });

        act.Should().NotThrow();
    }

    // ── PageCommand guards ───────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Page_RunCommand_Should_Ignore_Bad_Parameter(string? param)
    {
        var cmd = new PageCommand();
        var method = typeof(PageCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object?[] { param ?? string.Empty });

        act.Should().NotThrow();
    }

    [Fact]
    public void Page_RunCommand_With_No_Plugin_Should_Not_Throw()
    {
        Runbook.RunbookPlugin.Instance?.Unload();

        var cmd = new PageCommand();
        var method = typeof(PageCommand)
            .GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(cmd, new object[] { "next" });

        act.Should().NotThrow();
    }

    // ── RunbookAdjustment guards ─────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Adjustment_ApplyAdjustment_Should_Ignore_Bad_Parameter(string? param)
    {
        var adj = new RunbookAdjustment();
        var method = typeof(RunbookAdjustment)
            .GetMethod("ApplyAdjustment", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(adj, new object?[] { param ?? string.Empty, 1 });

        act.Should().NotThrow();
    }

    [Fact]
    public void Adjustment_ApplyAdjustment_With_No_Plugin_Should_Not_Throw()
    {
        Runbook.RunbookPlugin.Instance?.Unload();

        var adj = new RunbookAdjustment();
        var method = typeof(RunbookAdjustment)
            .GetMethod("ApplyAdjustment", BindingFlags.NonPublic | BindingFlags.Instance);

        var act = () => method!.Invoke(adj, new object[] { "roller", 5 });

        act.Should().NotThrow();
    }
}
