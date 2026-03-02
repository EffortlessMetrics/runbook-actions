using Runbook.Daemon;

namespace Runbook.LogiPlugin.Tests;

public class CoalescedAdjustmentBufferTests
{
    [Fact]
    public void Drain_WithNoPendingAdjustments_ReturnsZeroes()
    {
        var buffer = new CoalescedAdjustmentBuffer();

        var (roller, dial) = buffer.Drain();

        roller.Should().Be(0);
        dial.Should().Be(0);
    }

    [Fact]
    public void Enqueue_MixedKinds_AccumulatesIndependently()
    {
        var buffer = new CoalescedAdjustmentBuffer();

        buffer.Enqueue("roller", 6);
        buffer.Enqueue("dial", -4);
        buffer.Enqueue("roller", -1);
        buffer.Enqueue("dial", 3);

        var (roller, dial) = buffer.Drain();

        roller.Should().Be(5);
        dial.Should().Be(-1);
    }

    [Fact]
    public void Drain_ResetsPendingCounts()
    {
        var buffer = new CoalescedAdjustmentBuffer();
        buffer.Enqueue("roller", 2);
        buffer.Enqueue("dial", 7);

        _ = buffer.Drain();
        var (roller, dial) = buffer.Drain();

        roller.Should().Be(0);
        dial.Should().Be(0);
    }
}
