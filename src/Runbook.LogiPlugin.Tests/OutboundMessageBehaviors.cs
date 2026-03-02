using System.Text.Json;
using FluentAssertions;
using Runbook.Daemon;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests verifying outbound messages use snake_case.
///
/// The plugin serializes messages via anonymous objects + System.Text.Json.
/// These tests verify the shape matches protocol fixtures.
/// </summary>
public class OutboundMessageBehaviors
{
    // ── keypad_press ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(8)]
    public void KeypadPress_Should_Be_Snake_Case(int slot)
    {
        var msg = OutboundMessageFactory.KeypadPress(slot);
        var json = JsonSerializer.Serialize(msg);

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"keypad_press\"");
        json.Should().Contain("\"slot\"");
        json.Should().NotContain("\"keypadPress\"");
    }

    // ── dialpad_button_press ─────────────────────────────────────────

    [Theory]
    [InlineData("ctrl_c")]
    [InlineData("export")]
    [InlineData("esc")]
    [InlineData("enter")]
    public void DialpadButtonPress_Should_Be_Snake_Case(string button)
    {
        var msg = OutboundMessageFactory.DialpadButtonPress(button);
        var json = JsonSerializer.Serialize(msg);

        json.Should().Contain("\"dialpad_button_press\"");
        json.Should().Contain("\"button\"");
        json.Should().NotContain("\"dialpadButtonPress\"");
    }

    // ── page ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("prev")]
    [InlineData("next")]
    public void Page_Should_Have_Correct_Shape(string direction)
    {
        var msg = OutboundMessageFactory.Page(direction);
        var json = JsonSerializer.Serialize(msg);

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"page\"");
        json.Should().Contain($"\"{direction}\"");
    }

    // ── adjustment ───────────────────────────────────────────────────

    [Theory]
    [InlineData("roller", 3)]
    [InlineData("dial", -1)]
    [InlineData("roller", 0)]
    public void Adjustment_Should_Be_Snake_Case(string kind, int delta)
    {
        var msg = OutboundMessageFactory.Adjustment(kind, delta);
        var json = JsonSerializer.Serialize(msg);

        json.Should().Contain("\"type\"");
        json.Should().Contain("\"adjustment\"");
        json.Should().Contain("\"kind\"");
        json.Should().Contain("\"delta\"");
    }

    // ── hello ────────────────────────────────────────────────────────

    [Fact]
    public void Hello_Should_Be_Snake_Case()
    {
        var msg = OutboundMessageFactory.Hello("abc12345");
        var json = JsonSerializer.Serialize(msg);

        json.Should().Contain("\"client_id\"");
        json.Should().NotContain("\"clientId\"");
        json.Should().NotContain("\"Client\"");
    }

    // ── Round-trip: serialize matches fixture shape ───────────────────

    [Fact]
    public void All_Outbound_Types_Should_Have_Type_Field()
    {
        var messages = new object[]
        {
            OutboundMessageFactory.KeypadPress(0),
            OutboundMessageFactory.DialpadButtonPress("enter"),
            OutboundMessageFactory.Page("next"),
            OutboundMessageFactory.Adjustment("roller", 1),
            OutboundMessageFactory.Hello("x"),
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
