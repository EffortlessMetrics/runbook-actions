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
            "hooks_mode": "active",
            "pending_prompt": {
                "id": "prep_pr",
                "label": "Prep PR",
                "style": "queue"
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
        model.HooksMode.Should().Be("active");

        model.PendingPrompt.Should().NotBeNull();
        model.PendingPrompt!.Id.Should().Be("prep_pr");
        model.PendingPrompt.Label.Should().Be("Prep PR");
        model.PendingPrompt.Style.Should().Be("queue");

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
        model.HooksMode.Should().Be("absent");
        model.PendingPrompt.Should().BeNull();
        model.Keypad.Should().NotBeNull();
        model.Keypad.Slots.Should().BeEmpty();
    }
}
