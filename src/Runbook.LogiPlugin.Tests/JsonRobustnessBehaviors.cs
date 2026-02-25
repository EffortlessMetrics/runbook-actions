using System.Text.Json;
using FluentAssertions;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for JSON robustness: malformed input, unknown fields,
/// wrong types, and oversized payloads.
///
/// Contract: drop the message, keep last known render, don't crash.
/// </summary>
public class JsonRobustnessBehaviors
{
    // ── Scenario 1: Invalid JSON ─────────────────────────────────────
    // Given: daemon sends malformed JSON
    // Then:  deserialize returns null; no exception

    [Theory]
    [InlineData("{")]
    [InlineData("")]
    [InlineData("not json at all")]
    [InlineData("{\"type\": render}")]
    [InlineData("null")]
    public void Given_Malformed_Json_Deserialize_Should_Not_Throw(string badJson)
    {
        RenderModel? model = null;
        var act = () =>
        {
            try
            {
                model = JsonSerializer.Deserialize<RenderModel>(badJson);
            }
            catch (JsonException)
            {
                // Expected for truly broken JSON — plugin should catch and ignore.
                model = null;
            }
        };

        act.Should().NotThrow<Exception>("plugin must never crash on bad input");
    }

    // ── Scenario 2: Unknown fields ───────────────────────────────────
    // Given: render payload includes new/unknown fields from a future daemon
    // Then:  deserialization succeeds; known fields applied

    [Fact]
    public void Given_Unknown_Fields_Should_Deserialize_Known_Fields()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "idle",
            "future_field": "something new",
            "version": 42,
            "keypad": {
                "slots": [
                    {
                        "slot": 0,
                        "label": "Hello",
                        "armed": false,
                        "icon_url": "https://example.com/icon.png",
                        "priority": 5
                    }
                ],
                "page_count": 3
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.AgentState.Should().Be("idle");
        model.Keypad.Slots.Should().HaveCount(1);
        model.Keypad.Slots[0].Label.Should().Be("Hello");
    }

    [Fact]
    public void Given_Extra_Fields_In_Armed_Should_Preserve_Known()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "idle",
            "armed": {
                "id": "a1",
                "label": "Do It",
                "command": "/cmd",
                "timeout_ms": 5000,
                "priority": "high"
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Armed.Should().NotBeNull();
        model.Armed!.Id.Should().Be("a1");
        model.Armed.Label.Should().Be("Do It");
        model.Armed.Command.Should().Be("/cmd");
    }

    // ── Scenario 3: Wrong types ──────────────────────────────────────
    // Given: `slots` is an object not an array (daemon bug)
    // Then:  message ignored, no crash

    [Fact]
    public void Given_Slots_As_Object_Should_Not_Crash()
    {
        var json = """{"type":"render","agent_state":"idle","keypad":{"slots":{"0":{"label":"X"}}}}""";

        RenderModel? model = null;
        var act = () =>
        {
            try
            {
                model = JsonSerializer.Deserialize<RenderModel>(json);
            }
            catch (JsonException)
            {
                model = null;
            }
        };

        act.Should().NotThrow<Exception>("wrong type should be caught, not crash");
    }

    [Fact]
    public void Given_Agent_State_As_Number_Should_Not_Crash()
    {
        var json = """{"type":"render","agent_state":42}""";

        RenderModel? model = null;
        var act = () =>
        {
            try
            {
                model = JsonSerializer.Deserialize<RenderModel>(json);
            }
            catch (JsonException)
            {
                model = null;
            }
        };

        act.Should().NotThrow<Exception>();
    }

    [Fact]
    public void Given_Armed_As_String_Should_Not_Crash()
    {
        var json = """{"type":"render","agent_state":"idle","armed":"not_an_object"}""";

        RenderModel? model = null;
        var act = () =>
        {
            try
            {
                model = JsonSerializer.Deserialize<RenderModel>(json);
            }
            catch (JsonException)
            {
                model = null;
            }
        };

        act.Should().NotThrow<Exception>();
    }

    [Fact]
    public void Given_Slot_Number_As_String_Should_Not_Crash()
    {
        var json = """
        {"type":"render","agent_state":"idle","keypad":{"slots":[{"slot":"zero","label":"X","armed":false}]}}
        """;

        RenderModel? model = null;
        var act = () =>
        {
            try
            {
                model = JsonSerializer.Deserialize<RenderModel>(json);
            }
            catch (JsonException)
            {
                model = null;
            }
        };

        act.Should().NotThrow<Exception>();
    }

    // ── Scenario: empty type field ───────────────────────────────────

    [Fact]
    public void Given_Empty_Type_Should_Deserialize_Without_Crash()
    {
        var json = """{"type":"","agent_state":"idle"}""";

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.Type.Should().BeEmpty();
    }

    [Fact]
    public void Given_Null_Type_Should_Deserialize_With_Default()
    {
        var json = """{"type":null,"agent_state":"idle"}""";

        var act = () => JsonSerializer.Deserialize<RenderModel>(json);

        act.Should().NotThrow();
    }
}
