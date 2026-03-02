using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class DaemonProtocolBehaviors
{
    [Fact]
    public void HelloAck_With_Matching_Protocol_Should_Not_Be_Mismatch()
    {
        const string ackJson = """
            { "type": "hello_ack", "protocol": 1 }
            """;

        var result = DaemonProtocol.IsHelloAckProtocolMismatch(ackJson, out var detail);

        result.Should().BeFalse();
        detail.Should().BeNull();
    }

    [Fact]
    public void HelloAck_With_Mismatched_Protocol_Should_Return_Detail()
    {
        const string ackJson = """
            { "type": "hello_ack", "protocol": 2 }
            """;

        var result = DaemonProtocol.IsHelloAckProtocolMismatch(ackJson, out var detail);

        result.Should().BeTrue();
        detail.Should().Be("plugin=1 daemon=2");
    }

    [Fact]
    public void Non_HelloAck_Message_Should_Not_Be_Mismatch()
    {
        const string renderJson = """
            { "type": "render", "version": "1" }
            """;

        var result = DaemonProtocol.IsHelloAckProtocolMismatch(renderJson, out var detail);

        result.Should().BeFalse();
        detail.Should().BeNull();
    }
}
