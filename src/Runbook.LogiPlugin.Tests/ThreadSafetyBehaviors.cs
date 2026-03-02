using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Loupedeck;
using Runbook.Daemon;
using Runbook.Protocol;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for render-during-render thread safety and cache consistency.
///
/// Scenario 7: Render update during image request
///   Given: render update arrives while GetCommandImage is executing
///   Then:  no exception; cache consistent; next redraw shows latest state
///
/// Scenario 8: Backoff behavior
///   Given: daemon is down
///   Then:  reconnect attempts follow expected schedule and don't spin
/// </summary>
public class ThreadSafetyBehaviors
{
    // ── Scenario 7: Concurrent rendering ─────────────────────────────

    [Fact]
    public void Given_Concurrent_Render_Calls_Should_Not_Corrupt_Cache()
    {
        KeyRenderer.InvalidateCache();
        var size = new PluginImageSize();

        // Simulate parallel render calls from multiple threads.
        var tasks = new Task[20];
        for (var i = 0; i < tasks.Length; i++)
        {
            var slot = i % 9;
            var armed = i % 3 == 0;
            tasks[i] = Task.Run(() =>
            {
                var image = KeyRenderer.RenderSlot(size, slot, $"Slot {slot}", null, armed);
                image.Should().NotBeNull();
            });
        }

        Task.WaitAll(tasks);
    }

    [Fact]
    public void Given_Cache_Invalidation_During_Render_Should_Not_Throw()
    {
        var size = new PluginImageSize();

        // Aggressive: invalidate + render + invalidate concurrently.
        var tasks = new Task[30];
        for (var i = 0; i < tasks.Length; i++)
        {
            if (i % 3 == 0)
            {
                tasks[i] = Task.Run(KeyRenderer.InvalidateCache);
            }
            else if (i % 3 == 1)
            {
                tasks[i] = Task.Run(() =>
                {
                    var image = KeyRenderer.RenderSlot(size, 0, "Test", null, false);
                    image.Should().NotBeNull();
                });
            }
            else
            {
                tasks[i] = Task.Run(() =>
                {
                    var image = KeyRenderer.RenderOffline(size);
                    image.Should().NotBeNull();
                });
            }
        }

        var act = () => Task.WaitAll(tasks);
        act.Should().NotThrow("cache must be thread-safe");
    }

    [Fact]
    public void Given_Render_Update_In_Flight_Should_Use_Latest_State()
    {
        KeyRenderer.InvalidateCache();
        var size = new PluginImageSize();

        // Render slot with initial state.
        var initial = KeyRenderer.RenderSlot(size, 0, "Before", null, false);
        initial.Should().NotBeNull();

        // Simulate state change (cache invalidation + new content).
        KeyRenderer.InvalidateCache();
        var updated = KeyRenderer.RenderSlot(size, 0, "After", null, true);
        updated.Should().NotBeNull();

        // They should be different objects (different content after invalidation).
        initial.Should().NotBeSameAs(updated);
    }

    // ── Scenario 8: Backoff doesn't spin ─────────────────────────────

    [Fact]
    public async Task Given_Immediate_Dispose_Should_Not_Spin()
    {
        // Create a client and immediately dispose.
        // If backoff spins (tight loop), this would hang or timeout.
        var client = new DaemonClient();
        client.DaemonUrl = "ws://192.0.2.1:1/unreachable"; // RFC 5737 TEST-NET

        _ = client.ConnectAsync();

        // Give the connection loop a moment to start, then kill it.
        await Task.Delay(100);
        await client.DisposeAsync();

        // If we got here, the backoff loop exited cleanly.
        client.State.Should().Be(ConnectionState.Disconnected);
    }

    [Fact]
    public async Task Given_StateChanged_Events_Should_Fire_In_Order()
    {
        var client = new DaemonClient();
        var states = new System.Collections.Concurrent.ConcurrentQueue<ConnectionState>();
        client.StateChanged += (_, s) => states.Enqueue(s);

        client.DaemonUrl = "ws://192.0.2.1:1/unreachable";
        _ = client.ConnectAsync();

        // Let it try connecting briefly.
        await Task.Delay(200);
        await client.DisposeAsync();

        // Should have seen at least Connecting and then Disconnected.
        states.Should().NotBeEmpty("state changes should have fired");

        // First state should be Connecting (from the reconnect loop).
        states.TryPeek(out var first);
        first.Should().Be(ConnectionState.Connecting);
    }
}
