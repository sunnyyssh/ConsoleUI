namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs;

public abstract class UIElement
{
    public bool IsDrawn { get; private set; }
    
    public int Width { get; private set; }
    
    public int Height { get; private set; }

    protected internal DrawState? CurrentState { get; private set; }

    public OverlappingPriority Priority { get; }
    
    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        CurrentState = CurrentState?.HideOverlapWith(state) ?? state;
        RedrawElement?.Invoke(this, new RedrawElementEventArgs());
    }

    internal DrawState RequestDrawState(DrawOptions options)
    {
        return CurrentState ??= CreateDrawState(Width, Height);
    }

    protected abstract DrawState CreateDrawState(int width, int height);

    internal void OnDraw()
    {
        IsDrawn = true;
    }

    internal void OnRemove()
    {
        IsDrawn = false;
    }

    protected UIElement(int width, int height, OverlappingPriority priority)
    {
        Width = width;
        Height = height;
        Priority = priority;
    }
}