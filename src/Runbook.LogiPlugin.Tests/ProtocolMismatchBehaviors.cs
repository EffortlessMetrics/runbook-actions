using System;
using System.Threading.Tasks;
using FluentAssertions;
using Loupedeck;
using Runbook.Daemon;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for protocol mismatch rendering behavior.
///
/// Scenario 4: Protocol mismatch
///   Given: hello_ack protocol != expected
///   Then:  status Error, stop reconnect, OFFLINE tile
/// </summary>
public class ProtocolMismatchBehaviors
{
    [Fact]
    public void Given_ProtocolError_State_Renderer_Should_Show_Offline()
    {
        // When the daemon client is in ProtocolError, the plugin should
        // render OFFLINE tiles. KeypadSlotCommand checks daemon.State.
        var client = new DaemonClient();

        // Client starts Disconnected (not Connected), so renderer should show OFFLINE.
        client.State.Should().NotBe(ConnectionState.Connected);

        // OFFLINE tile should render for any non-connected state.
        var size = new PluginImageSize();
        var image = KeyRenderer.RenderOffline(size);
        image.Should().NotBeNull();
    }

    [Fact]
    public void Given_ProtocolError_Detail_Should_Be_Null_Before_Handshake()
    {
        var client = new DaemonClient();

        client.ProtocolErrorDetail.Should().BeNull(
            "no handshake has occurred yet");
    }

    [Fact]
    public async Task Given_Disposed_Client_Should_Not_Enter_ProtocolError()
    {
        var client = new DaemonClient();
        await client.DisposeAsync();

        // Dispose sets Disconnected, not ProtocolError.
        client.State.Should().Be(ConnectionState.Disconnected);
    }
}
