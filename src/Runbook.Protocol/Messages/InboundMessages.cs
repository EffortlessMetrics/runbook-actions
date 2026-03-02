using System.Text.Json;
using Runbook.Render;

namespace Runbook.Protocol.Messages;

public static class InboundMessages
{
    public static bool TryGetHelloAckProtocol(string json, out int daemonProtocol)
    {
        daemonProtocol = 0;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var type) || type.GetString() != "hello_ack")
            return false;

        if (!root.TryGetProperty("protocol", out var protocol))
            return false;

        daemonProtocol = protocol.GetInt32();
        return true;
    }

    public static bool TryParseRender(string json, out RenderModel? model)
    {
        model = null;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("type", out var typeProp) || typeProp.GetString() != "render")
            return false;

        model = JsonSerializer.Deserialize<RenderModel>(json);
        return model is not null;
    }
}
