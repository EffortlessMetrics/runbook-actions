using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class AdjustmentCoalescerBehaviors
{
    [Fact]
    public void Given_Roller_Adjustments_Should_Accumulate_And_Drain()
    {
        var coalescer = new AdjustmentCoalescer();

        coalescer.Enqueue("roller", 2);
        coalescer.Enqueue("roller", -1);

        var firstDrain = coalescer.Drain();

        firstDrain.Roller.Should().Be(1);
        firstDrain.Dial.Should().Be(0);
        coalescer.Drain().Should().Be((0, 0));
    }

    [Fact]
    public void Given_Dial_Adjustments_Should_Accumulate_And_Drain()
    {
        var coalescer = new AdjustmentCoalescer();

        coalescer.Enqueue("dial", 4);
        coalescer.Enqueue("dial", 3);

        var drained = coalescer.Drain();

        drained.Dial.Should().Be(7);
        drained.Roller.Should().Be(0);
    }

    [Fact]
    public void Given_Mixed_Adjustments_Should_Drain_Both_Independently()
    {
        var coalescer = new AdjustmentCoalescer();

        coalescer.Enqueue("roller", 5);
        coalescer.Enqueue("dial", -2);

        var drained = coalescer.Drain();

        drained.Roller.Should().Be(5);
        drained.Dial.Should().Be(-2);
    }
}
