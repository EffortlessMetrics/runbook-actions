using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for RunbookApplication: VS Code process binding.
/// GetProcessNames() is protected, so we test via reflection.
/// </summary>
public class RunbookApplicationBehaviors
{
    private static string[] GetProcessNames()
    {
        var app = new Runbook.RunbookApplication();
        var method = typeof(Runbook.RunbookApplication)
            .GetMethod("GetProcessNames", BindingFlags.NonPublic | BindingFlags.Instance);
        return (string[])method!.Invoke(app, null)!;
    }

    // ── Process name coverage ────────────────────────────────────────

    [Fact]
    public void Should_Include_VSCode_Stable()
    {
        GetProcessNames().Should().Contain("Code");
    }

    [Fact]
    public void Should_Include_VSCode_Insiders()
    {
        GetProcessNames().Should().Contain("Code - Insiders");
    }

    [Fact]
    public void Should_Return_Exactly_Two_Process_Names()
    {
        GetProcessNames().Should().HaveCount(2,
            "only VS Code stable and Insiders are supported");
    }

    // ── Negative: must NOT match unintended apps ─────────────────────

    [Fact]
    public void Should_Not_Include_Generic_Code_Variants()
    {
        var names = GetProcessNames();

        names.Should().NotContain("code",
            "lowercase 'code' is not a VS Code process name");
        names.Should().NotContain("VSCodium",
            "VSCodium support is deliberate opt-in, not accidental");
        names.Should().NotContain("Cursor",
            "Cursor support is deliberate opt-in, not accidental");
    }

    // ── Instantiation ────────────────────────────────────────────────

    [Fact]
    public void Should_Instantiate_Without_Throwing()
    {
        var act = () => new Runbook.RunbookApplication();

        act.Should().NotThrow();
    }
}
