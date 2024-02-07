using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs(RedrawState State);

public abstract class UIElement
{
    public bool IsDrawn { get; private set; } = false; // I don't know how to do it.
    
    public Sizing Sizing { get; private init; }
    
    public int? Height { get; private init; }
    
    public int? Width { get; private init; }
    
    public double? HeightRelation { get; private init; }
    
    public double? WidthRelation { get; private init; }

    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(RedrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        RedrawElement?.Invoke(this, new RedrawElementEventArgs(state));
    }
    
    protected internal abstract DrawState GetDrawState(DrawOptions options);

    internal void OnDraw() // Looks a bit weird but...
    {
        IsDrawn = true;
    }

    internal void OnRemove() // Looks a bit weird but...
    {
        IsDrawn = false;
    }
}