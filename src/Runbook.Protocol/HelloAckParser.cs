using System.Text.Json;

namespace Runbook.Protocol;

public static class HelloAckParser
{
    public static HelloAckParseResult Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return HelloAckParseResult.HandshakeAccepted();
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var t) || t.GetString() != "hello_ack")
        {
            return HelloAckParseResult.HandshakeAccepted();
        }

        if (!root.TryGetProperty("protocol", out var protocolValue))
        {
            return HelloAckParseResult.ProtocolMismatch($"plugin={OutboundMessages.ProtocolVersion} daemon=unknown");
        }

        var daemonProtocol = protocolValue.GetInt32();
        if (daemonProtocol != OutboundMessages.ProtocolVersion)
        {
            return HelloAckParseResult.ProtocolMismatch(
                $"plugin={OutboundMessages.ProtocolVersion} daemon={daemonProtocol}");
        }

        return HelloAckParseResult.HandshakeAccepted();
    }
}

public sealed record HelloAckParseResult(bool IsProtocolMismatch, string? Detail)
{
    public static HelloAckParseResult HandshakeAccepted() => new(false, null);

    public static HelloAckParseResult ProtocolMismatch(string detail) => new(true, detail);
}
