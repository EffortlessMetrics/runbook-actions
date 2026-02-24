using Loupedeck;

namespace Runbook;

/// <summary>
/// Associates the plugin with Visual Studio Code.
/// The SDK uses this to show VS Code-specific default profiles
/// ("Adapt to App") when Code is the foreground application.
/// </summary>
public sealed class RunbookApplication : ClientApplication
{
    public RunbookApplication()
    {
        // Match any process whose name contains "Code"
        // (handles Code.exe, Code - Insiders, etc.)
        this.AddProcess("Code", "com.microsoft.VSCode");
    }
}
