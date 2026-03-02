using FluentAssertions;
using Runbook.Protocol.Messages;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class InboundMessageBehaviors
{
    [Fact]
    public void HelloAck_Should_Parse_Protocol()
    {
        var ok = InboundMessages.TryGetHelloAckProtocol("{\"type\":\"hello_ack\",\"protocol\":2}", out var protocol);

        ok.Should().BeTrue();
        protocol.Should().Be(2);
    }

    [Fact]
    public void NonHelloAck_Should_Not_Parse_Protocol()
    {
        var ok = InboundMessages.TryGetHelloAckProtocol("{\"type\":\"render\"}", out _);

        ok.Should().BeFalse();
    }

    [Fact]
    public void RenderMessage_Should_Parse_Render_Model()
    {
        var json = "{\"type\":\"render\",\"agent_state\":\"idle\",\"hooks_mode\":\"present\",\"keypad\":{\"slots\":[{\"slot\":0,\"label\":\"Run\",\"sublabel\":\"Now\",\"armed\":true}]}}";

        var ok = InboundMessages.TryParseRender(json, out var model);

        ok.Should().BeTrue();
        model.Should().NotBeNull();
        model!.Keypad.Slots.Should().HaveCount(1);
        model.Keypad.Slots[0].Label.Should().Be("Run");
    }
}
