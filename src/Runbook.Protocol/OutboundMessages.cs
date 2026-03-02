using System.Text.Json.Serialization;

namespace Runbook.Protocol;

public interface IOutboundMessage
{
    [JsonPropertyName("type")]
    string Type { get; }
}

public sealed record HelloMessage(
    [property: JsonPropertyName("client")] string Client,
    [property: JsonPropertyName("protocol")] int Protocol,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("client_id")] string ClientId) : IOutboundMessage
{
    [JsonPropertyName("type")]
    public string Type => "hello";
}

public sealed record KeypadPressMessage([property: JsonPropertyName("slot")] int Slot) : IOutboundMessage
{
    [JsonPropertyName("type")]
    public string Type => "keypad_press";
}

public sealed record DialpadButtonPressMessage([property: JsonPropertyName("button")] string Button) : IOutboundMessage
{
    [JsonPropertyName("type")]
    public string Type => "dialpad_button_press";
}

public sealed record PageMessage([property: JsonPropertyName("direction")] string Direction) : IOutboundMessage
{
    [JsonPropertyName("type")]
    public string Type => "page";
}

public sealed record AdjustmentMessage(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("delta")] int Delta) : IOutboundMessage
{
    [JsonPropertyName("type")]
    public string Type => "adjustment";
}
