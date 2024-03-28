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
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        
        CurrentState = CurrentState?.HideOverlapWith(state) ?? state;
        RedrawElement?.Invoke(this, new RedrawElementEventArgs());
    }

    protected internal DrawState RequestDrawState(DrawOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        
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
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, null);
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, null);
        
        Width = width;
        Height = height;
        Priority = priority;
    }
}