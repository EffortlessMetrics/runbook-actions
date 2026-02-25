using Loupedeck;

namespace Runbook;

/// <summary>
/// Associates the plugin with Visual Studio Code.
/// The SDK uses this to activate VS Code-specific default profiles
/// ("Adapt to App") when Code is the foreground application.
/// </summary>
public sealed class RunbookApplication : ClientApplication
{
    public RunbookApplication()
    {
    }

    protected override string[] GetProcessNames() => new[]
    {
        "Code",            // VS Code stable
        "Code - Insiders", // VS Code Insiders
    };
}
