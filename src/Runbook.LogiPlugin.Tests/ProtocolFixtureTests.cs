using System.IO;
using System.Text.Json;
using FluentAssertions;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// Round-trip tests against protocol/fixtures/*.json.
/// Enforces snake_case field naming and structural correctness.
/// </summary>
public class ProtocolFixtureTests
{
    private static string FixturePath(string name) =>
        Path.Combine("..", "..", "..", "..", "..", "protocol", "fixtures", name);

    [Fact]
    public void Render_Fixture_Deserializes_Correctly()
    {
        var json = File.ReadAllText(FixturePath("render.json"));
        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.Type.Should().Be("render");
        model.AgentState.Should().Be("idle");
        model.HooksMode.Should().Be("active");
        model.PendingPrompt.Should().NotBeNull();
        model.PendingPrompt!.Id.Should().Be("prep_pr");
        model.PendingPrompt.Style.Should().Be("queue");
        model.Keypad.Slots.Should().HaveCount(2);
        model.Keypad.Slots[0].Armed.Should().BeTrue();
        model.Keypad.Slots[1].Armed.Should().BeFalse();
    }

    [Fact]
    public void Render_Fixture_Is_Snake_Case()
    {
        var json = File.ReadAllText(FixturePath("render.json"));

        // Must NOT contain camelCase variants of known fields.
        json.Should().NotContain("\"agentState\"");
        json.Should().NotContain("\"armedPrompt\"");
        json.Should().NotContain("\"subLabel\"");

        // Must contain snake_case.
        json.Should().Contain("\"agent_state\"");
    }

    [Fact]
    public void Hello_Fixture_Has_Required_Fields()
    {
        var json = File.ReadAllText(FixturePath("hello.json"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("hello");
        root.GetProperty("client").GetString().Should().Be("logi");
        root.GetProperty("protocol").GetInt32().Should().Be(1);
        root.TryGetProperty("client_id", out _).Should().BeTrue();
    }

    [Fact]
    public void HelloAck_Fixture_Has_Required_Fields()
    {
        var json = File.ReadAllText(FixturePath("hello_ack.json"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("hello_ack");
        root.GetProperty("protocol").GetInt32().Should().Be(1);
    }

    [Fact]
    public void KeypadPress_Fixture_Is_Snake_Case()
    {
        var json = File.ReadAllText(FixturePath("keypad_press.json"));
        json.Should().Contain("\"keypad_press\"");
        json.Should().NotContain("\"keypadPress\"");

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("slot").GetInt32().Should().Be(3);
    }

    [Fact]
    public void DialpadButton_Fixture_Is_Snake_Case()
    {
        var json = File.ReadAllText(FixturePath("dialpad_button.json"));
        json.Should().Contain("\"dialpad_button_press\"");
        json.Should().NotContain("\"dialpadButtonPress\"");

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("button").GetString().Should().Be("enter");
    }
}
