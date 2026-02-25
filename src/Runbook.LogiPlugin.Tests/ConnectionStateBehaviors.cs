using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for ConnectionState enum completeness and expected values.
/// </summary>
public class ConnectionStateBehaviors
{
    [Fact]
    public void Should_Have_Four_States()
    {
        var values = System.Enum.GetValues<ConnectionState>();

        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ConnectionState.Disconnected)]
    [InlineData(ConnectionState.Connecting)]
    [InlineData(ConnectionState.Connected)]
    [InlineData(ConnectionState.ProtocolError)]
    public void Should_Contain_Expected_State(ConnectionState state)
    {
        System.Enum.IsDefined(state).Should().BeTrue();
    }

    [Fact]
    public void Default_Should_Be_Disconnected()
    {
        default(ConnectionState).Should().Be(ConnectionState.Disconnected);
    }
}
