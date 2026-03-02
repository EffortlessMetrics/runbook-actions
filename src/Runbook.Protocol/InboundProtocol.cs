using System.Text.Json;

namespace Runbook.Protocol;

public static class InboundProtocol
{
    public static string? GetType(JsonElement root)
        => root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

    public static bool TryGetHelloAckProtocol(JsonElement root, out int protocol)
    {
        protocol = default;
        if (GetType(root) != "hello_ack" || !root.TryGetProperty("protocol", out var protocolProp))
            return false;

        protocol = protocolProp.GetInt32();
        return true;
    }

    public static bool IsRender(JsonElement root)
        => GetType(root) == "render";
}
