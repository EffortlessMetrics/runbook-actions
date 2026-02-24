using Loupedeck;

namespace Runbook.Render;

/// <summary>
/// Renders keypad slot images. Caches bitmaps by content to avoid
/// unnecessary redraws.
/// </summary>
public static class KeyRenderer
{
    private record struct CacheKey(int Slot, string Label, string? Sublabel, bool Armed, bool Offline);

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<CacheKey, BitmapImage> _cache = new();

    /// <summary>Render (or return cached) image for a keypad slot.</summary>
    public static BitmapImage RenderSlot(
        PluginImageSize size, int slot,
        string label, string? sublabel, bool armed)
    {
        var key = new CacheKey(slot, label, sublabel, armed, false);
        return _cache.GetOrAdd(key, _ => DrawSlot(size, label, sublabel, armed));
    }

    /// <summary>Render the OFFLINE tile.</summary>
    public static BitmapImage RenderOffline(PluginImageSize size)
    {
        var key = new CacheKey(-1, "OFFLINE", null, false, true);
        return _cache.GetOrAdd(key, _ =>
        {
            using var bb = new BitmapBuilder(size);
            bb.Clear(BitmapColor.Black);
            bb.DrawText("OFFLINE");
            return bb.ToImage();
        });
    }

    /// <summary>Clear the cache (e.g., on theme change).</summary>
    public static void InvalidateCache() => _cache.Clear();

    private static BitmapImage DrawSlot(
        PluginImageSize size, string label, string? sublabel, bool armed)
    {
        using var bb = new BitmapBuilder(size);
        bb.Clear(armed ? new BitmapColor(0, 102, 204) : BitmapColor.Black);

        var textColor = armed ? BitmapColor.White : new BitmapColor(200, 200, 200);

        // SDK guidance: do NOT force fontSize — the runtime picks best size per device.
        bb.DrawText(label, color: textColor);

        if (!string.IsNullOrEmpty(sublabel))
        {
            bb.DrawText(sublabel!, color: textColor);
        }

        if (armed)
        {
            bb.DrawRectangle(0, 0, size.Width - 1, size.Height - 1, BitmapColor.White);
            bb.DrawRectangle(1, 1, size.Width - 3, size.Height - 3, BitmapColor.White);
        }

        return bb.ToImage();
    }
}
