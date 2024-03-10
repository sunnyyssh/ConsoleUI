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

    protected UIElement(int width, int height, OverlappingPriority priority)
        : this(new Size(width, height), priority)
    {}

    protected UIElement(int width, double heightRelation, OverlappingPriority priority)
        : this(new Size(width, heightRelation), priority)
    {}

    protected UIElement(double widthRelation, int height, OverlappingPriority priority)
        : this(new Size(widthRelation, height), priority)
    {}

    protected UIElement(double widthRelation, double heightRelation, OverlappingPriority priority)
        : this(new Size(widthRelation, heightRelation), priority)
    {}

    protected UIElement(Size size, OverlappingPriority priority)
    {
        Size = size;
        Priority = priority;
    }
}