using FluentAssertions;
using Runbook.Adjustments;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for RunbookAdjustment behaviors.
/// </summary>
public class AdjustmentBehaviors
{
    [Fact]
    public void Given_RunbookAdjustment_Should_Have_Display_Name()
    {
        var adj = new RunbookAdjustment();

        adj.Name.Should().Be("Runbook Adjustment");
    }

    [Fact]
    public void Given_RunbookAdjustment_Should_Instantiate_Without_Throwing()
    {
        var act = () => new RunbookAdjustment();

        act.Should().NotThrow();
    }
}
