using FluentAssertions;
using Runbook;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for RunbookPlugin lifecycle.
///
/// Because the plugin extends the SDK's Plugin base class (stubs),
/// we can test Load/Unload, singleton wiring, and status mapping
/// without a real SDK runtime.
/// </summary>
public class RunbookPluginBehaviors
{
    // ── Load lifecycle ───────────────────────────────────────────────

    [Fact]
    public void Given_Load_Should_Set_Singleton_Instance()
    {
        var plugin = new RunbookPlugin();

        plugin.Load();

        RunbookPlugin.Instance.Should().BeSameAs(plugin);

        plugin.Unload();
    }

    [Fact]
    public void Given_Load_Should_Create_DaemonClient()
    {
        var plugin = new RunbookPlugin();

        plugin.Load();

        plugin.Daemon.Should().NotBeNull();

        plugin.Unload();
    }

    [Fact]
    public void Given_Load_DaemonClient_Should_Have_Default_Url()
    {
        var plugin = new RunbookPlugin();

        plugin.Load();

        // Plugin settings stub returns false, so DaemonClient uses its default URL.
        plugin.Daemon.DaemonUrl.Should().Be("ws://127.0.0.1:29381/ws");

        plugin.Unload();
    }

    [Fact]
    public void Given_Load_DaemonClient_Should_Have_Generated_ClientId()
    {
        var plugin = new RunbookPlugin();

        plugin.Load();

        plugin.Daemon.ClientId.Should().NotBeNullOrWhiteSpace();
        plugin.Daemon.ClientId.Should().HaveLength(8);

        plugin.Unload();
    }

    // ── Unload lifecycle ─────────────────────────────────────────────

    [Fact]
    public void Given_Unload_Should_Clear_Singleton()
    {
        var plugin = new RunbookPlugin();
        plugin.Load();

        plugin.Unload();

        RunbookPlugin.Instance.Should().BeNull();
    }

    [Fact]
    public void Given_Double_Unload_Should_Not_Throw()
    {
        var plugin = new RunbookPlugin();
        plugin.Load();

        var act = () =>
        {
            plugin.Unload();
            plugin.Unload();
        };

        act.Should().NotThrow("double unload must be safe");
    }

    [Fact]
    public void Given_Unload_Without_Load_Should_Not_Throw()
    {
        var plugin = new RunbookPlugin();

        var act = () => plugin.Unload();

        act.Should().NotThrow("unload before load must be safe");
    }

    // ── Singleton replacement ────────────────────────────────────────

    [Fact]
    public void Given_Second_Load_Should_Replace_Singleton()
    {
        var first = new RunbookPlugin();
        first.Load();

        var second = new RunbookPlugin();
        second.Load();

        RunbookPlugin.Instance.Should().BeSameAs(second,
            "latest Load() wins the singleton");

        second.Unload();
        first.Unload();
    }
}
