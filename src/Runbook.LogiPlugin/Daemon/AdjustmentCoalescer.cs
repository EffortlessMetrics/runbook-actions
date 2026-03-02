using System.Threading;

namespace Runbook.Daemon;

/// <summary>
/// Thread-safe accumulator for high-frequency dial/roller adjustments.
/// </summary>
internal sealed class AdjustmentCoalescer
{
    private int _pendingRollerDelta;
    private int _pendingDialDelta;

    public void Enqueue(string kind, int delta)
    {
        if (kind == "roller")
            Interlocked.Add(ref _pendingRollerDelta, delta);
        else
            Interlocked.Add(ref _pendingDialDelta, delta);
    }

    public (int Roller, int Dial) Drain()
    {
        var roller = Interlocked.Exchange(ref _pendingRollerDelta, 0);
        var dial = Interlocked.Exchange(ref _pendingDialDelta, 0);
        return (roller, dial);
    }
}
