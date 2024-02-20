
namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs(DrawState State);

public abstract class UIElement
{
    public bool IsDrawn { get; private set; } = false; // I don't know how to do it. It's kinda cringe.
    
    public Size Size { get; }

    protected int ActualWidth { get; private set; }
    
    protected int ActualHeight { get; private set; }

    protected DrawState? PreviousState;

    public OverlappingPriority OverlappingPriority { get; }
    
    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        PreviousState = PreviousState?.OverlapWith(state) ?? state;
        RedrawElement?.Invoke(this, new RedrawElementEventArgs(state));
    }

    internal DrawState RequestDrawState(DrawOptions options)
    {
        ActualWidth = options.Width;
        ActualHeight = options.Height;
        return PreviousState ??= GetDrawState(ActualWidth, ActualHeight);
    }

    protected abstract DrawState GetDrawState(int width, int height);

    internal void OnDraw()
    {
        IsDrawn = true;
    }

    internal void OnRemove()
    {
        IsDrawn = false;
    }

    protected UIElement(Size size, OverlappingPriority overlappingPriority)
    {
        Size = size;
        OverlappingPriority = overlappingPriority;
    }
}