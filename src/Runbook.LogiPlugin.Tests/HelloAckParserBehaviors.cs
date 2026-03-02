using FluentAssertions;
using Runbook.Protocol;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class HelloAckParserBehaviors
{
    [Fact]
    public void Matching_Protocol_Should_Be_Accepted()
    {
        var ack = """
                  {"type":"hello_ack","protocol":1}
                  """;

        var result = HelloAckParser.Parse(ack);

        result.IsProtocolMismatch.Should().BeFalse();
        result.Detail.Should().BeNull();
    }

    [Fact]
    public void Mismatched_Protocol_Should_Return_Detail()
    {
        var ack = """
                  {"type":"hello_ack","protocol":2}
                  """;

        var result = HelloAckParser.Parse(ack);

        result.IsProtocolMismatch.Should().BeTrue();
        result.Detail.Should().Be("plugin=1 daemon=2");
    }

    [Fact]
    public void Non_Ack_Messages_Should_Not_Block_Handshake()
    {
        var result = HelloAckParser.Parse("{" + "\"type\":\"render\"}");

        result.IsProtocolMismatch.Should().BeFalse();
    }
}
