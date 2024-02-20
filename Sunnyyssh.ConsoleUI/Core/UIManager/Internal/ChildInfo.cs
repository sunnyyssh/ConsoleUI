using System.Collections;
using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

internal sealed class OrderedOverlappingCollection : IEnumerable<ChildInfo>
{
    private readonly List<ChildInfo> _children = new();

    public bool Contains(ChildInfo childInfo) => _children.Contains(childInfo);

    public bool Add(ChildInfo childInfo)
    {
        if (Contains(childInfo))
            return false;
        int lastLowerPriorityIndex = _children
            .FindLastIndex(ch => ch.Child.OverlappingPriority <= childInfo.Child.OverlappingPriority);
        // Inserting after all children with lower or equal priority.
        // If there are no such children index will be -1 so it will be inserted at 0 position as expected.
        _children.Insert(lastLowerPriorityIndex + 1, childInfo);
        return true;
    }

    public bool Remove(ChildInfo childInfo)
    {
        return _children.Remove(childInfo);
    }
    
    public IEnumerator<ChildInfo> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_children).GetEnumerator();
}

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

    private readonly OrderedOverlappingCollection _overlapping = new();
    private readonly OrderedOverlappingCollection _underlying = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="possibleOverlapping"><see cref="ChildInfo"/> instance to check overlapping with.
    /// If <see cref="equalPriorityOverlapping"/> is true then this instance is overlapping.</param>
    /// <param name="equalPriorityOverlapping"></param>
    /// <returns>True if successfully added. False otherwise</returns>
    public bool AddIfOverlapping(ChildInfo possibleOverlapping, bool equalPriorityOverlapping)
    {
        if (possibleOverlapping == this)
            return false;
        if (_overlapping.Contains(possibleOverlapping))
            return false;
        if (IsIntersectedWith(possibleOverlapping))
        {
            if (Child.OverlappingPriority < possibleOverlapping.Child.OverlappingPriority
                || equalPriorityOverlapping && Child.OverlappingPriority == possibleOverlapping.Child.OverlappingPriority)
            {
                // Adding new ChildInfo instance that overlaps this.
                _overlapping.Add(possibleOverlapping);
                // this instance is actually the underlying of possibleUnderlying.
                possibleOverlapping._underlying.Add(this);
                return true;
            }
            
            // possibleOverlapping is overlapped by this instance. 
            possibleOverlapping._overlapping.Add(this);
            _underlying.Add(possibleOverlapping);
            return true;
        }

        return false;
    }

    internal InternalDrawState TransformState(InternalDrawState rowDrawState)
    {
        // If some elements overlap current we should handle it.
        var resultDrawState = OverlapUnderlyingWithState(rowDrawState);
        // If current one overlaps others state we also should handle it.
        resultDrawState = SubtractStateWithOverlapping(resultDrawState);
        return resultDrawState;
    }

    public DrawState TransformState(DrawState rowDrawState)
    {
        return new DrawState(
            TransformState(
                rowDrawState
                    .ToInternal(0, 0)));
    }

    private InternalDrawState SubtractStateWithOverlapping(InternalDrawState bareState)
    {
        return _overlapping.Aggregate(bareState,
            (accumulatedState, nextOverlapping) =>
                accumulatedState.SubtractWith(nextOverlapping.PreviousState ?? InternalDrawState.Empty)
        );
    }
    
    private InternalDrawState OverlapUnderlyingWithState(InternalDrawState bareState)
    {
        return InternalDrawState.Combine(_underlying
            .Where(ch => ch.PreviousState is not null)
            .Select(ch => ch.PreviousState!)
            .Append(bareState)
            .ToArray()
        ).Crop(Left, Top, Width, Height);
    }
    
    public bool RemoveIfOverlapping(ChildInfo childInfo)
    {
        return _overlapping.Remove(childInfo) && childInfo._underlying.Remove(this) || 
               childInfo._overlapping.Remove(this) && _underlying.Remove(childInfo);
    }

    public InternalDrawState CreateErasingState()
    {
        // All underlying states combined.
        var combinedUnderlying = InternalDrawState.Combine(_underlying
            .Where(ch => ch.PreviousState is not null)
            .Select(ch => ch.PreviousState!)
            .ToArray()
        ).Crop(Left, Top, Width, Height);
        
        // Creating state of all pixels just default-backgrounded.
        InternalDrawState defaultBackgroundedState = new(
            Enumerable.Range(0, Height)
                .Select(i => new PixelLine(Left, Top + i,
                    Enumerable.Range(0, Width)
                        .Select(_ => new PixelInfo(Color.Default))
                        .ToArray()))
                .ToArray()
        );
        // filling non-visible pixels by overlapping over defaultBackgroundedState
        var filledUnderlying = defaultBackgroundedState.OverlapWith(combinedUnderlying);
        
        // Removing pixels that are overlapped.
        var resultState = SubtractStateWithOverlapping(filledUnderlying);
        
        return resultState;
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
        IsFocusable = child is IFocusable;
    }
}