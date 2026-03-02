using System.Text.Json;

namespace Runbook.Daemon;

internal static class DaemonHandshake
{
    private const int ExpectedProtocol = 1;

    public static bool TryGetProtocolError(string? ackJson, out string? detail)
    {
        detail = null;
        if (ackJson is null)
            return false;

        try
        {
            using var doc = JsonDocument.Parse(ackJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty) || typeProperty.GetString() != "hello_ack")
                return false;

            if (!root.TryGetProperty("protocol", out var protocolProperty))
                return false;

            var daemonProtocol = protocolProperty.GetInt32();
            if (daemonProtocol == ExpectedProtocol)
                return false;

            detail = $"plugin={ExpectedProtocol} daemon={daemonProtocol}";
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
