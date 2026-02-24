using Loupedeck;

namespace Runbook.Actions;

/// <summary>
/// Page navigation commands for cycling through prompt pages.
/// Assign with parameter "prev" or "next".
/// </summary>
public sealed class PageCommand : PluginDynamicCommand
{
    public PageCommand()
        : base(displayName: "Runbook Page", description: "Cycle prompt pages", groupName: "Runbook")
    {
    }

    protected override void RunCommand(string actionParameter)
    {
        if (string.IsNullOrWhiteSpace(actionParameter))
            return;

        _ = RunbookPlugin.Instance?.Daemon.SendPageAsync(actionParameter);
    }
}
