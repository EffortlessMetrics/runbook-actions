using System;
using System.Threading;

namespace Runbook.Daemon;

internal sealed class AdjustmentCoalescer : IDisposable
{
    private int _pendingRollerDelta;
    private int _pendingDialDelta;
    private Timer? _timer;

    public void Start(Action<(int roller, int dial)> onFlush)
    {
        _timer = new Timer(_ =>
        {
            var roller = Interlocked.Exchange(ref _pendingRollerDelta, 0);
            var dial = Interlocked.Exchange(ref _pendingDialDelta, 0);
            onFlush((roller, dial));
        }, null, 16, 16);
    }

    public void Enqueue(string kind, int delta)
    {
        if (kind == "roller")
            Interlocked.Add(ref _pendingRollerDelta, delta);
        else
            Interlocked.Add(ref _pendingDialDelta, delta);
    }

    public (int roller, int dial) Drain()
        => (Interlocked.Exchange(ref _pendingRollerDelta, 0), Interlocked.Exchange(ref _pendingDialDelta, 0));

    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
        Drain();
    }
}
