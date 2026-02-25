using FluentAssertions;
using Loupedeck;
using Runbook.Render;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

/// <summary>
/// BDD tests for KeyRenderer behaviors.
/// </summary>
public class KeyRendererBehaviors
{
    private readonly PluginImageSize _size = new();

    // ── Given an offline state ───────────────────────────────────────

    [Fact]
    public void Given_Offline_Should_Render_Non_Null_Image()
    {
        var image = KeyRenderer.RenderOffline(_size);

        image.Should().NotBeNull();
    }

    [Fact]
    public void Given_Offline_Called_Twice_Should_Return_Cached_Instance()
    {
        KeyRenderer.InvalidateCache();

        var first = KeyRenderer.RenderOffline(_size);
        var second = KeyRenderer.RenderOffline(_size);

        first.Should().BeSameAs(second, "OFFLINE tile should be cached");
    }

    // ── Given a normal slot ──────────────────────────────────────────

    [Fact]
    public void Given_Normal_Slot_Should_Render_Non_Null_Image()
    {
        KeyRenderer.InvalidateCache();

        var image = KeyRenderer.RenderSlot(_size, slot: 0, label: "Test", sublabel: null, armed: false);

        image.Should().NotBeNull();
    }

    [Fact]
    public void Given_Same_Content_Should_Return_Cached_Instance()
    {
        KeyRenderer.InvalidateCache();

        var first = KeyRenderer.RenderSlot(_size, 0, "Test", null, false);
        var second = KeyRenderer.RenderSlot(_size, 0, "Test", null, false);

        first.Should().BeSameAs(second, "identical content should hit cache");
    }

    [Fact]
    public void Given_Different_Label_Should_Return_Different_Instance()
    {
        KeyRenderer.InvalidateCache();

        var a = KeyRenderer.RenderSlot(_size, 0, "Alpha", null, false);
        var b = KeyRenderer.RenderSlot(_size, 0, "Beta", null, false);

        a.Should().NotBeSameAs(b, "different labels produce different images");
    }

    // ── Given an armed slot ──────────────────────────────────────────

    [Fact]
    public void Given_Armed_Slot_Should_Render_Non_Null_Image()
    {
        KeyRenderer.InvalidateCache();

        var image = KeyRenderer.RenderSlot(_size, 3, "Armed", "sub", armed: true);

        image.Should().NotBeNull();
    }

    [Fact]
    public void Given_Armed_Vs_Unarmed_Should_Differ()
    {
        KeyRenderer.InvalidateCache();

        var armed = KeyRenderer.RenderSlot(_size, 0, "Same", null, armed: true);
        var unarmed = KeyRenderer.RenderSlot(_size, 0, "Same", null, armed: false);

        armed.Should().NotBeSameAs(unarmed, "armed state changes the visual");
    }

    // ── Given a sublabel ─────────────────────────────────────────────

    [Fact]
    public void Given_Sublabel_Should_Render_Non_Null_Image()
    {
        KeyRenderer.InvalidateCache();

        var image = KeyRenderer.RenderSlot(_size, 1, "Label", "sublabel", false);

        image.Should().NotBeNull();
    }

    [Fact]
    public void Given_Sublabel_Vs_No_Sublabel_Should_Differ()
    {
        KeyRenderer.InvalidateCache();

        var withSub = KeyRenderer.RenderSlot(_size, 0, "L", "sub", false);
        var withoutSub = KeyRenderer.RenderSlot(_size, 0, "L", null, false);

        withSub.Should().NotBeSameAs(withoutSub);
    }

    // ── Given cache invalidation ─────────────────────────────────────

    [Fact]
    public void Given_Cache_Invalidated_Should_Produce_New_Instance()
    {
        var first = KeyRenderer.RenderSlot(_size, 0, "X", null, false);

        KeyRenderer.InvalidateCache();

        var second = KeyRenderer.RenderSlot(_size, 0, "X", null, false);

        // After invalidation, the factory runs again, producing a new object.
        // (BitmapImage is a stub here, so all instances are equal in value,
        //  but they should be distinct object references.)
        first.Should().NotBeSameAs(second, "cache was invalidated");
    }
}
