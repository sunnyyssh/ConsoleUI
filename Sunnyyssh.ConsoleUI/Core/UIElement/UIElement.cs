using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs(RedrawState State);



public abstract class UIElement
{
    // TODO Overlapping priority.
    
    public bool IsDrawn { get; private set; } = false; // I don't know how to do it. It's kinda cringe.

    public Size Size { get; }

    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(RedrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        RedrawElement?.Invoke(this, new RedrawElementEventArgs(state));
    }
    
    protected internal abstract DrawState RequestDrawState(DrawOptions options);

    internal void OnDraw() // Looks a bit weird but...
    {
        // TODO resolve where UIElement should take its actual size when it tries to redraw
        IsDrawn = true;
    }

    internal void OnRemove() // Looks a bit weird but...
    {
        IsDrawn = false;
    }

    protected UIElement(Size size)
    {
        Size = size;
    }
}