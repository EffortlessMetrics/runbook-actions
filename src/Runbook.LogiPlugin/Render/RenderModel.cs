using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Runbook.Render;

public sealed class RenderModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "render";

    [JsonPropertyName("agent_state")]
    public string AgentState { get; set; } = "unknown";

    [JsonPropertyName("hooks_mode")]
    public string HooksMode { get; set; } = "absent";

    [JsonPropertyName("pending_prompt")]
    public PendingPromptState? PendingPrompt { get; set; }

    [JsonPropertyName("keypad")]
    public KeypadRender Keypad { get; set; } = new();
}

public sealed class PendingPromptState
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("style")]
    public string Style { get; set; } = string.Empty;
}

public sealed class KeypadRender
{
    [JsonPropertyName("slots")]
    public List<KeypadSlotRender> Slots { get; set; } = new();
}

public sealed class KeypadSlotRender
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("sublabel")]
    public string? Sublabel { get; set; }

    [JsonPropertyName("armed")]
    public bool Armed { get; set; }
}
