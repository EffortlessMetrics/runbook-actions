using Loupedeck;

namespace Runbook.Adjustments;

/// <summary>
/// Single adjustment action, parameterized by kind:
/// - "dial" (scroll)
/// - "roller" (cycle terminals)
/// </summary>
public sealed class RunbookAdjustment : PluginDynamicAdjustment
{
    public RunbookAdjustment()
        : base(displayName: "Runbook Adjustment", description: "Dial/roller input", groupName: "Runbook")
    {
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        // diff is signed, in detents.
        if (string.IsNullOrEmpty(actionParameter))
            return;

        _ = RunbookPlugin.Instance?.Daemon.SendAdjustmentAsync(actionParameter, diff);
    }
}
