using System;
using System.Threading;

namespace Runbook.Daemon;

/// <summary>
/// Coalesces dial/roller adjustment deltas and periodically flushes aggregated values.
/// </summary>
internal sealed class AdjustmentCoalescer : IDisposable
{
    private readonly Action<string, int> _onFlush;
    private readonly Timer _timer;
    private int _pendingRollerDelta;
    private int _pendingDialDelta;
    private bool _disposed;

    public AdjustmentCoalescer(TimeSpan interval, Action<string, int> onFlush)
    {
        _onFlush = onFlush ?? throw new ArgumentNullException(nameof(onFlush));
        _timer = new Timer(_ => Flush(), null, interval, interval);
    }

    public void Enqueue(string kind, int delta)
    {
        if (_disposed)
            return;

        if (kind == "roller")
            Interlocked.Add(ref _pendingRollerDelta, delta);
        else
            Interlocked.Add(ref _pendingDialDelta, delta);
    }

    public void Flush()
    {
        if (_disposed)
            return;

        var roller = Interlocked.Exchange(ref _pendingRollerDelta, 0);
        var dial = Interlocked.Exchange(ref _pendingDialDelta, 0);

        if (roller != 0)
            _onFlush("roller", roller);

        if (dial != 0)
            _onFlush("dial", dial);
    }

    public void Dispose()
    {
        _disposed = true;
        _timer.Dispose();
        _ = Interlocked.Exchange(ref _pendingRollerDelta, 0);
        _ = Interlocked.Exchange(ref _pendingDialDelta, 0);
    }
}
