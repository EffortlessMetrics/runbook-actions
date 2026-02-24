using System.Text.Json;
using FluentAssertions;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class RenderModelTests
{
    [Fact]
    public void Should_Deserialize_Valid_Render_Payload()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "idle",
            "armed": {
                "id": "prep_pr",
                "label": "Prep PR",
                "command": "/runbook:prep-pr"
            },
            "keypad": {
                "slots": [
                    {
                        "slot": 1,
                        "label": "Break Task",
                        "sublabel": "break",
                        "armed": true
                    }
                ]
            }
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.Type.Should().Be("render");
        model.AgentState.Should().Be("idle");

        model.Armed.Should().NotBeNull();
        model.Armed!.Id.Should().Be("prep_pr");
        model.Armed.Label.Should().Be("Prep PR");
        model.Armed.Command.Should().Be("/runbook:prep-pr");

        model.Keypad.Should().NotBeNull();
        model.Keypad.Slots.Should().HaveCount(1);

        var slot = model.Keypad.Slots[0];
        slot.Slot.Should().Be(1);
        slot.Label.Should().Be("Break Task");
        slot.Sublabel.Should().Be("break");
        slot.Armed.Should().BeTrue();
    }

    [Fact]
    public void Should_Handle_Missing_Optional_Fields()
    {
        var json = """
        {
            "type": "render",
            "agent_state": "unknown"
        }
        """;

        var model = JsonSerializer.Deserialize<RenderModel>(json);

        model.Should().NotBeNull();
        model!.Type.Should().Be("render");
        model.AgentState.Should().Be("unknown");
        model.Armed.Should().BeNull();
        model.Keypad.Should().NotBeNull();
        model.Keypad.Slots.Should().BeEmpty();
    }
}
