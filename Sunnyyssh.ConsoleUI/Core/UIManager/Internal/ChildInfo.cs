using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerDisplay("{DebuggerDisplay}")]
internal sealed class ChildInfo
{
    private string DebuggerDisplay => $"{Child}: Left={Left}; Top={Top}; Width={Width}; Height={Height}";
    public UIElement Child { get; private init; }
    public int Left { get; private init; }
    public int Top { get; private init; }
    public int Width { get; private init; }
    public int Height { get; private init; }
    public bool IsFocusable { get; private init; }

    // It's very important to set this Property only after handling the state
    // because it will be ambigious if somebody is intersected with state which has not even been handled.
    public InternalDrawState? PreviousState { get; set; }

    private readonly List<ChildInfo> _overlapping = new();

    public ChildInfo[] Overlapping => _overlapping.ToArray();

    public void AddIfOverlapping(ChildInfo possibleOverlapping, bool equalPriorityOverlapping)
    {
        if (possibleOverlapping == this)
            return;
        if (_overlapping.Contains(possibleOverlapping))
            return;
        if (IsIntersectedWith(possibleOverlapping))
        {
            if (Child.OverlappingPriority < possibleOverlapping.Child.OverlappingPriority 
                || equalPriorityOverlapping && Child.OverlappingPriority == possibleOverlapping.Child.OverlappingPriority)
            {
                _overlapping.Add(possibleOverlapping);
            }
        }
    }

    public InternalDrawState SubtractStateWithOverlapping(InternalDrawState bareState)
    {
        return _overlapping.Aggregate(bareState,
            (accumulatedState, nextOverlapping) =>
                accumulatedState.SubtractWith(nextOverlapping?.PreviousState ?? InternalDrawState.Empty)
        );
    }
    
    public bool RemoveIfOverlapping(ChildInfo childInfo)
    {
        return _overlapping.Remove(childInfo);
    }

    public bool IsIntersectedWith(ChildInfo child)
    {
        bool horizontalIntersected = child.Left < Left + Width && child.Left + child.Width > Left;
        bool verticalIntersected = child.Top < Top + Height && child.Top + child.Height > Top;
        return horizontalIntersected && verticalIntersected;
    }
    
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