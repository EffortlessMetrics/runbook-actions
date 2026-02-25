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
        if (string.IsNullOrEmpty(actionParameter))
            return;

        // Roller events are high-frequency: coalesce them.
        // Dial uses direct send (optional fallback binding).
        RunbookPlugin.Instance?.Daemon.EnqueueAdjustment(actionParameter, diff);
    }
}
