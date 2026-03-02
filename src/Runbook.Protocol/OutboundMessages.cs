using System.Text.Json;

namespace Runbook.Protocol;

public static class OutboundMessages
{
    public static HelloMessage Hello(string clientId)
        => new("logi", 1, "0.1.0", clientId);

    public static KeypadPressMessage KeypadPress(int slot)
        => new(slot);

    public static DialpadButtonPressMessage DialpadButtonPress(string button)
        => new(button);

    public static PageMessage Page(string direction)
        => new(direction);

    public static AdjustmentMessage Adjustment(string kind, int delta)
        => new(kind, delta);

    public static string ToJson<TMessage>(TMessage message)
        => JsonSerializer.Serialize(message);
}

public sealed record KeypadPressMessage(int slot)
{
    public string type { get; } = "keypad_press";
}

public sealed record DialpadButtonPressMessage(string button)
{
    public string type { get; } = "dialpad_button_press";
}

public sealed record PageMessage(string direction)
{
    public string type { get; } = "page";
}

public sealed record AdjustmentMessage(string kind, int delta)
{
    public string type { get; } = "adjustment";
}

public sealed record HelloMessage(string client, int protocol, string version, string client_id)
{
    public string type { get; } = "hello";
}
