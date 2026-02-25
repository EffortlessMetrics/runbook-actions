using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for RenderModel deserialization edge cases and invariants.
/// </summary>
public class RenderModelBehaviors
{
    // ── Given a full render payload ──────────────────────────────────

    [Fact]
    public void Given_Full_Payload_Should_Deserialize_All_Fields()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "working",
            "armed": {
                "id": "a1",
                "label": "Do Something",
                "command": "/cmd"
            },
            "keypad": {
                "slots": [
                    { "slot": 0, "label": "Slot 0", "sublabel": "sub", "armed": true },
                    { "slot": 1, "label": "Slot 1", "armed": false }
                ]
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.Type.Should().Be("render");
        model.AgentState.Should().Be("working");
        model.Armed.Should().NotBeNull();
        model.Armed!.Id.Should().Be("a1");
        model.Armed.Label.Should().Be("Do Something");
        model.Armed.Command.Should().Be("/cmd");
        model.Keypad.Slots.Should().HaveCount(2);
    }

    // ── Given missing optional fields ────────────────────────────────

    [Fact]
    public void Given_No_Armed_Should_Deserialize_As_Null()
    {
        var json = """{"type":"render","agent_state":"idle"}""";

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Armed.Should().BeNull();
    }

    [Fact]
    public void Given_No_Keypad_Should_Default_To_Empty_Slots()
    {
        var json = """{"type":"render","agent_state":"idle"}""";

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Keypad.Should().NotBeNull();
        model.Keypad.Slots.Should().BeEmpty();
    }

    [Fact]
    public void Given_Empty_Slots_Array_Should_Deserialize()
    {
        var json = """{"type":"render","agent_state":"idle","keypad":{"slots":[]}}""";

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Keypad.Slots.Should().BeEmpty();
    }

    // ── Given slot with missing sublabel ──────────────────────────────

    [Fact]
    public void Given_Slot_Without_Sublabel_Should_Be_Null()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "idle",
            "keypad": {
                "slots": [{ "slot": 0, "label": "Hello", "armed": false }]
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);
        var slot = model!.Keypad.Slots[0];

        slot.Sublabel.Should().BeNull();
    }

    // ── Given multiple slots ─────────────────────────────────────────

    [Fact]
    public void Given_Nine_Slots_Should_All_Deserialize()
    {
        var slots = new List<object>();
        for (var i = 0; i < 9; i++)
            slots.Add(new { slot = i, label = $"Slot {i}", armed = i % 2 == 0 });

        var payload = new { type = "render", agent_state = "idle", keypad = new { slots } };
        var json = JsonSerializer.Serialize(payload);

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Keypad.Slots.Should().HaveCount(9);
        model.Keypad.Slots[0].Armed.Should().BeTrue();
        model.Keypad.Slots[1].Armed.Should().BeFalse();
        model.Keypad.Slots[8].Armed.Should().BeTrue();
    }

    // ── Given various agent states ───────────────────────────────────

    [Theory]
    [InlineData("idle")]
    [InlineData("working")]
    [InlineData("waiting")]
    [InlineData("error")]
    [InlineData("unknown")]
    public void Given_Agent_State_Should_Round_Trip(string state)
    {
        var json = $"{{\"type\":\"render\",\"agent_state\":\"{state}\"}}";

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.AgentState.Should().Be(state);
    }

    // ── Given armed prompt fields ────────────────────────────────────

    [Fact]
    public void Given_Armed_Prompt_Should_Preserve_All_Fields()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "idle",
            "armed": {
                "id": "prompt_xyz",
                "label": "Execute Plan",
                "command": "/execute"
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model!.Armed!.Id.Should().Be("prompt_xyz");
        model.Armed.Label.Should().Be("Execute Plan");
        model.Armed.Command.Should().Be("/execute");
    }

    // ── snake_case invariant ─────────────────────────────────────────

    [Fact]
    public void Given_Serialized_Model_Should_Use_Snake_Case()
    {
        var model = new RenderModel
        {
            AgentState = "idle",
            Armed = new ArmedPrompt { Id = "x", Label = "Y", Command = "/z" }
        };

        var json = JsonSerializer.Serialize(model);

        json.Should().Contain("\"agent_state\"");
        json.Should().NotContain("\"AgentState\"");
        json.Should().NotContain("\"agentState\"");
    }
}
