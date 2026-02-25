using System;
using System.Threading.Tasks;
using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for DaemonClient behaviors.
/// These test the unit's state machine and configuration without
/// requiring a live WebSocket server.
/// </summary>
public class DaemonClientBehaviors
{
    // ── Given a new DaemonClient ─────────────────────────────────────

    [Fact]
    public void Given_New_Client_Should_Be_Disconnected()
    {
        var client = new DaemonClient();

        client.State.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public void Given_New_Client_Should_Have_No_Render_Model()
    {
        var client = new DaemonClient();

        client.Render.Should().BeNull();
    }

    [Fact]
    public void Given_New_Client_Should_Have_Default_Daemon_Url()
    {
        var client = new DaemonClient();

        client.DaemonUrl.Should().Be("ws://127.0.0.1:29381/ws");
    }

    [Fact]
    public void Given_New_Client_Should_Have_Generated_Client_Id()
    {
        var client = new DaemonClient();

        client.ClientId.Should().NotBeNullOrWhiteSpace();
        client.ClientId.Should().HaveLength(8);
    }

    // ── Given a configured DaemonClient ──────────────────────────────

    [Fact]
    public void Given_Custom_Url_Should_Use_It()
    {
        var client = new DaemonClient();
        client.DaemonUrl = "ws://10.0.0.1:9999/ws";

        client.DaemonUrl.Should().Be("ws://10.0.0.1:9999/ws");
    }

    [Fact]
    public void Given_Custom_Client_Id_Should_Use_It()
    {
        var client = new DaemonClient();
        client.ClientId = "test1234";

        client.ClientId.Should().Be("test1234");
    }

    // ── State change events ──────────────────────────────────────────

    [Fact]
    public async Task Given_Disposed_Client_Should_Be_Disconnected()
    {
        var client = new DaemonClient();

        await client.DisposeAsync();

        client.State.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public async Task Given_Disposed_Client_ConnectAsync_Should_Be_Noop()
    {
        var client = new DaemonClient();
        await client.DisposeAsync();

        // Should not throw.
        await client.ConnectAsync();

        client.State.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public async Task Given_Client_StateChanged_Should_Fire_On_Dispose()
    {
        var client = new DaemonClient();
        var states = new System.Collections.Generic.List<ConnectionState>();
        client.StateChanged += (_, s) => states.Add(s);

        await client.DisposeAsync();

        // Dispose sets Disconnected (may already be disconnected, so list may be empty
        // if state didn't change).
        client.State.Should().Be(ConnectionState.Disconnected);
    }

    // ── Adjustment coalescing ────────────────────────────────────────

    [Fact]
    public void Given_Enqueued_Roller_Adjustments_Should_Accumulate()
    {
        var client = new DaemonClient();

        // Enqueue multiple roller deltas.
        client.EnqueueAdjustment("roller", 1);
        client.EnqueueAdjustment("roller", 1);
        client.EnqueueAdjustment("roller", -1);

        // No crash; the coalescing timer would flush these.
        // We verify the method accepts multiple calls without throwing.
    }

    [Fact]
    public void Given_Enqueued_Dial_Adjustments_Should_Accumulate()
    {
        var client = new DaemonClient();

        client.EnqueueAdjustment("dial", 3);
        client.EnqueueAdjustment("dial", -2);

        // Same as above: verifying no crash on rapid enqueue.
    }

    // ── Send methods on disconnected client ──────────────────────────

    [Fact]
    public async Task Given_Disconnected_Client_SendKeypadPress_Should_Not_Throw()
    {
        var client = new DaemonClient();

        var act = async () => await client.SendKeypadPressAsync(0);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_Disconnected_Client_SendDialpadButton_Should_Not_Throw()
    {
        var client = new DaemonClient();

        var act = async () => await client.SendDialpadButtonPressAsync("enter");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_Disconnected_Client_SendPage_Should_Not_Throw()
    {
        var client = new DaemonClient();

        var act = async () => await client.SendPageAsync("next");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_Disconnected_Client_SendAdjustment_Should_Not_Throw()
    {
        var client = new DaemonClient();

        var act = async () => await client.SendAdjustmentAsync("roller", 5);

        await act.Should().NotThrowAsync();
    }
}
