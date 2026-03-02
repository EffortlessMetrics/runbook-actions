using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class DaemonInfrastructureBehaviors
{
    [Fact]
    public void Given_Mixed_Adjustments_When_Drain_Then_Roller_And_Dial_Are_Separated()
    {
        using var coalescer = new AdjustmentCoalescer();

        coalescer.Enqueue("roller", 5);
        coalescer.Enqueue("dial", -2);
        coalescer.Enqueue("roller", -3);

        var deltas = coalescer.Drain();

        deltas.roller.Should().Be(2);
        deltas.dial.Should().Be(-2);
    }

    [Fact]
    public void Given_Drained_Adjustments_When_Drain_Again_Then_Values_Are_Zeroed()
    {
        using var coalescer = new AdjustmentCoalescer();
        coalescer.Enqueue("roller", 10);

        _ = coalescer.Drain();
        var secondDrain = coalescer.Drain();

        secondDrain.Should().Be((0, 0));
    }

    [Theory]
    [InlineData("{\"type\":\"hello_ack\",\"protocol\":1}", false, null)]
    [InlineData("{\"type\":\"hello_ack\",\"protocol\":2}", true, "plugin=1 daemon=2")]
    [InlineData("{\"type\":\"render\"}", false, null)]
    [InlineData(null, false, null)]
    [InlineData("not-json", false, null)]
    public void Given_HelloAck_Payload_When_Validating_Protocol_Then_Error_Is_Reported_Only_For_Mismatch(
        string? ackJson,
        bool expectedMismatch,
        string? expectedDetail)
    {
        var mismatch = DaemonHandshake.TryGetProtocolError(ackJson, out var detail);

        mismatch.Should().Be(expectedMismatch);
        detail.Should().Be(expectedDetail);
    }
}
