using System;

namespace Loupedeck;

public abstract class Plugin
{
    public virtual void Load() { }
    public virtual void Unload() { }
    protected void OnStatusChanged(PluginStatus status, string message) { }
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
    protected PluginDynamicAdjustment(string displayName, string description, string groupName) { }
    protected virtual void ApplyAdjustment(string actionParameter, int diff) { }
    public void AdjustmentValueChanged(string actionParameter) { }
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
    public static readonly BitmapColor White = new BitmapColor(255, 255, 255);
    public static readonly BitmapColor Black = new BitmapColor(0, 0, 0);

    public BitmapColor(byte r, byte g, byte b, byte a = 255) { }
}

public class BitmapBuilder : IDisposable
{
    public BitmapBuilder(PluginImageSize size) { }
    public void Clear() { }
    public void FillRectangle(int x, int y, int w, int h, BitmapColor color) { }
    public void DrawRectangle(int x, int y, int w, int h, BitmapColor color) { }
    public void DrawText(string text, int x, int y, int width, int height, BitmapColor? color = null, int fontSize = 12) { }
    public BitmapImage ToImage() => new BitmapImage();
    public void Dispose() { }
}
