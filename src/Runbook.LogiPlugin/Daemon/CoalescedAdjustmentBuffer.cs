using System.Threading;

namespace Runbook.Daemon;

internal sealed class CoalescedAdjustmentBuffer
{
    private int _pendingRollerDelta;
    private int _pendingDialDelta;

    public void Enqueue(string kind, int delta)
    {
        if (kind == "roller")
        {
            Interlocked.Add(ref _pendingRollerDelta, delta);
            return;
        }

        Interlocked.Add(ref _pendingDialDelta, delta);
    }

    public (int roller, int dial) Drain()
    {
        var roller = Interlocked.Exchange(ref _pendingRollerDelta, 0);
        var dial = Interlocked.Exchange(ref _pendingDialDelta, 0);
        return (roller, dial);
    }
}
