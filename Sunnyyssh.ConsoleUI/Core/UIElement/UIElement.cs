using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Handles redrawing <see cref="UIElement"/>.
/// </summary>
internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

/// <summary>
/// Args of redrawing <see cref="UIElement"/>.
/// </summary>
internal record RedrawElementEventArgs;

/// <summary>
/// Represents the element of UI that handles drawing itself. 
/// </summary>
public abstract class UIElement
{
    /// <summary>
    /// Indicates if it is drawn.
    /// </summary>
    [MemberNotNullWhen(true, nameof(CurrentState))]
    public bool IsDrawn { get; private set; }

    public bool IsStateInitialized { get; private set; }
    
    /// <summary>
    /// The absolute width. (Counted in characters).
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The absolute height. (Counted in characters).
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Current draw state.
    /// </summary>
    protected internal DrawState? CurrentState { get; private set; }

    /// <summary>
    /// Specifies a priority of overlapping <see cref="UIElement"/> children when they are placed intersected.
    /// </summary>
    public OverlappingPriority Priority { get; }
    
    /// <summary>
    /// Event that occurs when this instance should be redrawn.
    /// </summary>
    internal event RedrawElementEventHandler? RedrawElement;

    /// <summary>
    /// Redraws it.
    /// </summary>
    /// <param name="state">The state to redraw with. (This state hides previous one. <see cref="DrawState.HideOverlap"/> is used.)</param>
    /// <param name="hideOverlap">If true then pixels just hide previous. If false it's overlapped smartly.</param>
    protected void Redraw(DrawState state, bool hideOverlap = true)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        IsStateInitialized = true;
        
        CurrentState = hideOverlap 
            ? CurrentState?.HideOverlapWith(state) ?? state
            : CurrentState?.OverlapWith(state) ?? state;
        if (IsDrawn)
        {
            RedrawElement?.Invoke(this, new RedrawElementEventArgs());
        }
    }

    /// <summary>
    /// Requests draw state of the element.
    /// </summary>
    /// <param name="options">Specifies drawing options.</param>
    /// <returns>Draw state of this element.</returns>
    protected internal DrawState RequestDrawState(DrawOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        IsStateInitialized = true;
        
        return CurrentState ??= CreateDrawState();
    }

    protected abstract DrawState CreateDrawState();

    /// <summary>
    /// Handles being drawn at start.
    /// </summary>
    protected internal virtual void OnDraw()
    {
        IsDrawn = true;
    }

    /// <summary>
    /// Handles being removed.
    /// </summary>
    protected internal virtual void OnRemove()
    {
        IsDrawn = false;
    }

    /// <summary>
    /// Creates <see cref="UIElement"/> instance.
    /// </summary>
    /// <param name="width">The absolute width. (Counted in characters).</param>
    /// <param name="height">The absolute height. (Counted in characters).</param>
    /// <param name="priority">Overlapping priority.</param>
    /// <exception cref="ArgumentOutOfRangeException">Sizes are incorrect.</exception>
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