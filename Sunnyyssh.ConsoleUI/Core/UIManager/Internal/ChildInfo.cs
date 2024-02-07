namespace Sunnyyssh.ConsoleUI;

internal sealed class ChildInfo
{
    public UIElement Child { get; private init; }
    public int Left { get; private init; }
    public int Top { get; private init; }
    public int Width { get; private init; }
    public int Height { get; private init; }
    public bool IsFocusable { get; private init; }
    
    public ChildInfo(UIElement child, int left, int top, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        Child = child;
        Left = left;
        Top = top;
        Width = width;
        Height = height;
        // ReSharper disable once SuspiciousTypeConversion.Global
        IsFocusable = child is IFocusable;
    }
}