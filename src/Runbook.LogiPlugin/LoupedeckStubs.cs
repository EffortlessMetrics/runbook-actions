using System;

namespace Loupedeck;

/// <summary>
/// Loupedeck SDK stubs for compilation without the real SDK assemblies.
/// These are the minimum surface required by the plugin's code.
/// Replace with real SDK references when packaging for distribution.
/// </summary>

public abstract class Plugin
{
    public virtual bool UsesApplicationApiOnly => false;
    public virtual void Load() { }
    public virtual void Unload() { }
    protected void OnPluginStatusChanged(PluginStatus status, string message, string? url = null) { }
    public void ActionImageChanged() { }
    public void ActionImageChanged(string actionName, string parameter) { }
}

public enum PluginStatus
{
    Normal,
    Warning,
    Error
}

public abstract class PluginDynamicCommand
{
    public string Name { get; set; } = string.Empty;
    protected PluginDynamicCommand(string displayName, string description, string groupName)
    {
        Name = displayName;
    }
    protected virtual bool OnLoad() => true;
    protected virtual void RunCommand(string actionParameter) { }
    protected virtual BitmapImage? GetCommandImage(string actionParameter, PluginImageSize imageSize) => null;
    public void ActionImageChanged() { }
}

public abstract class PluginDynamicAdjustment
{
    public string Name { get; set; } = string.Empty;
    protected PluginDynamicAdjustment(string displayName, string description, string groupName)
    {
        Name = displayName;
    }
    protected virtual void ApplyAdjustment(string actionParameter, int diff) { }
    public void AdjustmentValueChanged(string actionParameter) { }
}

public abstract class ClientApplication
{
    protected void AddProcess(string processName, string? bundleId = null) { }
}

public struct PluginImageSize
{
    public int Width => 60;
    public int Height => 60;
}

public class BitmapImage
{
}

public class BitmapColor
{
    public static readonly BitmapColor White = new(255, 255, 255);
    public static readonly BitmapColor Black = new(0, 0, 0);

    public BitmapColor(byte r, byte g, byte b, byte a = 255) { }
}

public class BitmapBuilder : IDisposable
{
    public BitmapBuilder(PluginImageSize size) { }
    public void Clear(BitmapColor? color = null) { }
    public void FillRectangle(int x, int y, int w, int h, BitmapColor color) { }
    public void DrawRectangle(int x, int y, int w, int h, BitmapColor color) { }
    public void DrawText(string text, BitmapColor? color = null) { }
    public void DrawText(string text, int x, int y, int width, int height, BitmapColor? color = null, int fontSize = 12) { }
    public BitmapImage ToImage() => new();
    public void Dispose() { }
}
