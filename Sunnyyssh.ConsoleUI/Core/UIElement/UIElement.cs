
namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs;

public abstract class UIElement
{
    public bool IsDrawn { get; private set; }
    
    public Size Size { get; }

    protected int ActualWidth { get; private set; }
    
    protected int ActualHeight { get; private set; }

    protected internal DrawState? CurrentState { get; private set; }

    public OverlappingPriority OverlappingPriority { get; }
    
    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        CurrentState = CurrentState?.HideOverlapWith(state) ?? state;
        RedrawElement?.Invoke(this, new RedrawElementEventArgs());
    }

    internal DrawState RequestDrawState(DrawOptions options)
    {
        ActualWidth = options.Width;
        ActualHeight = options.Height;
        return CurrentState ??= GetDrawState(ActualWidth, ActualHeight);
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