using System;
using System.Collections.Generic;
using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class AdjustmentCoalescerBehaviors
{
    [Fact]
    public void Given_Multiple_Queued_Deltas_When_Flush_Then_Emits_Coalesced_Values()
    {
        var flushed = new List<(string kind, int delta)>();
        using var coalescer = new AdjustmentCoalescer(TimeSpan.FromMinutes(1),
            (kind, delta) => flushed.Add((kind, delta)));

        coalescer.Enqueue("roller", 3);
        coalescer.Enqueue("roller", -1);
        coalescer.Enqueue("dial", -2);
        coalescer.Enqueue("dial", 5);

        coalescer.Flush();

        flushed.Should().BeEquivalentTo(new[]
        {
            ("roller", 2),
            ("dial", 3)
        });
    }

    [Fact]
    public void Given_Flush_Without_Pending_Deltas_When_Flush_Then_Emits_Nothing()
    {
        var flushed = new List<(string kind, int delta)>();
        using var coalescer = new AdjustmentCoalescer(TimeSpan.FromMinutes(1),
            (kind, delta) => flushed.Add((kind, delta)));

        coalescer.Flush();

        flushed.Should().BeEmpty();
    }

    [Fact]
    public void Given_Disposed_Coalescer_When_Enqueue_And_Flush_Then_Acts_As_NoOp()
    {
        var flushed = new List<(string kind, int delta)>();
        var coalescer = new AdjustmentCoalescer(TimeSpan.FromMinutes(1),
            (kind, delta) => flushed.Add((kind, delta)));

        coalescer.Dispose();

        var act = () =>
        {
            coalescer.Enqueue("roller", 8);
            coalescer.Flush();
        };

        act.Should().NotThrow();
        flushed.Should().BeEmpty();
    }
}
