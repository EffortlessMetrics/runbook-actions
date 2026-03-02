using System.Text.Json;
using FluentAssertions;
using Runbook.Protocol;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests verifying outbound protocol message contracts stay snake_case.
/// </summary>
public class OutboundMessageBehaviors
{
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(8)]
    public void KeypadPress_Should_Be_Snake_Case(int slot)
    {
        var json = JsonSerializer.Serialize(OutboundMessages.KeypadPress(slot));

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"keypad_press\"");
        json.Should().Contain("\"slot\"");
        json.Should().NotContain("\"keypadPress\"");
    }

    [Theory]
    [InlineData("ctrl_c")]
    [InlineData("export")]
    [InlineData("esc")]
    [InlineData("enter")]
    public void DialpadButtonPress_Should_Be_Snake_Case(string button)
    {
        var json = JsonSerializer.Serialize(OutboundMessages.DialpadButtonPress(button));

        json.Should().Contain("\"dialpad_button_press\"");
        json.Should().Contain("\"button\"");
        json.Should().NotContain("\"dialpadButtonPress\"");
    }

    [Theory]
    [InlineData("prev")]
    [InlineData("next")]
    public void Page_Should_Have_Correct_Shape(string direction)
    {
        var json = JsonSerializer.Serialize(OutboundMessages.Page(direction));

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"page\"");
        json.Should().Contain($"\"{direction}\"");
    }

    [Theory]
    [InlineData("roller", 3)]
    [InlineData("dial", -1)]
    [InlineData("roller", 0)]
    public void Adjustment_Should_Be_Snake_Case(string kind, int delta)
    {
        var json = JsonSerializer.Serialize(OutboundMessages.Adjustment(kind, delta));

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"adjustment\"");
        json.Should().Contain("\"kind\"");
        json.Should().Contain("\"delta\"");
    }

    [Fact]
    public void Hello_Should_Be_Snake_Case()
    {
        var json = JsonSerializer.Serialize(OutboundMessages.Hello("abc12345"));

        json.Should().Contain("\"client_id\"");
        json.Should().NotContain("\"clientId\"");
        json.Should().NotContain("\"Client\"");
    }

    [Fact]
    public void All_Outbound_Types_Should_Have_Type_Field()
    {
        var messages = new object[]
        {
            OutboundMessages.KeypadPress(0),
            OutboundMessages.DialpadButtonPress("enter"),
            OutboundMessages.Page("next"),
            OutboundMessages.Adjustment("roller", 1),
            OutboundMessages.Hello("x"),
        };

        foreach (var msg in messages)
        {
            var json = JsonSerializer.Serialize(msg);
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("type", out _).Should().BeTrue(
                $"every outbound message must have a 'type' field: {json}");
        }
    }
}
