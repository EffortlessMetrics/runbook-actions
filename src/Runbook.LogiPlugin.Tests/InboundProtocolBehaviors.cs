using System.Text.Json;
using FluentAssertions;
using Runbook.Protocol;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class InboundProtocolBehaviors
{
    [Fact]
    public void TryGetHelloAckProtocol_Should_Return_True_For_HelloAck()
    {
        using var doc = JsonDocument.Parse("""
            {"type":"hello_ack","protocol":1}
            """);

        var ok = InboundProtocol.TryGetHelloAckProtocol(doc.RootElement, out var protocol);

        ok.Should().BeTrue();
        protocol.Should().Be(1);
    }

    [Fact]
    public void TryGetHelloAckProtocol_Should_Return_False_For_NonHelloAck()
    {
        using var doc = JsonDocument.Parse("""
            {"type":"render"}
            """);

        var ok = InboundProtocol.TryGetHelloAckProtocol(doc.RootElement, out _);

        ok.Should().BeFalse();
    }

    [Fact]
    public void IsRender_Should_Detect_Render_Message()
    {
        using var doc = JsonDocument.Parse("""
            {"type":"render"}
            """);

        InboundProtocol.IsRender(doc.RootElement).Should().BeTrue();
    }
}
