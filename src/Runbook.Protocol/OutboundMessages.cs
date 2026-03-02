using System.Text.Json.Serialization;

namespace Runbook.Protocol;

public static class OutboundMessages
{
    public const string ClientName = "logi";
    public const int ProtocolVersion = 1;
    public const string ClientVersion = "0.1.0";

    public static HelloMessage Hello(string clientId)
        => new(ClientName, ProtocolVersion, ClientVersion, clientId);

    public static KeypadPressMessage KeypadPress(int slot)
        => new(slot);

    public static DialpadButtonPressMessage DialpadButtonPress(string button)
        => new(button);

    public static PageMessage Page(string direction)
        => new(direction);

    public static AdjustmentMessage Adjustment(string kind, int delta)
        => new(kind, delta);
}

public sealed record HelloMessage(
    [property: JsonPropertyName("client")] string Client,
    [property: JsonPropertyName("protocol")] int Protocol,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("client_id")] string ClientId)
{
    [JsonPropertyName("type")]
    public string Type => "hello";
}

public sealed record KeypadPressMessage([property: JsonPropertyName("slot")] int Slot)
{
    [JsonPropertyName("type")]
    public string Type => "keypad_press";
}

public sealed record DialpadButtonPressMessage([property: JsonPropertyName("button")] string Button)
{
    [JsonPropertyName("type")]
    public string Type => "dialpad_button_press";
}

public sealed record PageMessage([property: JsonPropertyName("direction")] string Direction)
{
    [JsonPropertyName("type")]
    public string Type => "page";
}

public sealed record AdjustmentMessage(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("delta")] int Delta)
{
    [JsonPropertyName("type")]
    public string Type => "adjustment";
}
