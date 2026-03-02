namespace Runbook.Daemon;

internal static class OutboundMessageFactory
{
    public static object Hello(string clientId) => new
    {
        type = "hello",
        client = "logi",
        protocol = DaemonProtocol.ExpectedProtocol,
        version = "0.1.0",
        client_id = clientId
    };

    public static object KeypadPress(int slot) => new { type = "keypad_press", slot };

    public static object DialpadButtonPress(string button) => new { type = "dialpad_button_press", button };

    public static object Page(string direction) => new { type = "page", direction };

    public static object Adjustment(string kind, int delta) => new { type = "adjustment", kind, delta };
}
