using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

public abstract class UIElement
{
    public bool IsDrawn { get; private set; } = false; // I don't know how to do it.
    
    public Sizing Sizing { get; private init; }
    
    public int? Height { get; private init; }
    
    public int? Width { get; private init; }
    
    public double? WidthRelation { get; private init; }
    
    public double? HeightRelation { get; private init; }

    internal event RemoveElementEventHandler? RemoveElement;

    internal event RedrawElementEventHandler? RedrawElement;

    protected void Remove(RemoveOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RemoveElement?.Invoke(this, new RemoveElementEventsArgs(options));
    }

    protected void Redraw(RedrawOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RedrawElement?.Invoke(this, new RedrawElementEventArgs(options));
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