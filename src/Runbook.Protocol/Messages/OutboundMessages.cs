using System.Text.Json.Serialization;

namespace Runbook.Protocol.Messages;

public static class OutboundMessages
{
    public static HelloMessage Hello(string clientId)
        => new("hello", "logi", 1, "0.1.0", clientId);

    public static KeypadPressMessage KeypadPress(int slot)
        => new("keypad_press", slot);

    public static DialpadButtonPressMessage DialpadButtonPress(string button)
        => new("dialpad_button_press", button);

    public static PageMessage Page(string direction)
        => new("page", direction);

    public static AdjustmentMessage Adjustment(string kind, int delta)
        => new("adjustment", kind, delta);
}

public sealed record HelloMessage(
    string Type,
    string Client,
    int Protocol,
    string Version,
    [property: JsonPropertyName("client_id")] string ClientId);

public sealed record KeypadPressMessage(string Type, int Slot);

public sealed record DialpadButtonPressMessage(string Type, string Button);

public sealed record PageMessage(string Type, string Direction);

public sealed record AdjustmentMessage(string Type, string Kind, int Delta);
