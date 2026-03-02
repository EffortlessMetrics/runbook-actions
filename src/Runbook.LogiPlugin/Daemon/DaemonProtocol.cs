using System.Text.Json;

namespace Runbook.Daemon;

internal static class DaemonProtocol
{
    public const int ExpectedProtocol = 1;

    public static bool IsHelloAckProtocolMismatch(string ackJson, out string? detail)
    {
        detail = null;

        using var doc = JsonDocument.Parse(ackJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var type) || type.GetString() != "hello_ack")
            return false;

        if (!root.TryGetProperty("protocol", out var protocolElement))
            return false;

        var daemonProtocol = protocolElement.GetInt32();
        if (daemonProtocol == ExpectedProtocol)
            return false;

        detail = $"plugin={ExpectedProtocol} daemon={daemonProtocol}";
        return true;
    }
}
