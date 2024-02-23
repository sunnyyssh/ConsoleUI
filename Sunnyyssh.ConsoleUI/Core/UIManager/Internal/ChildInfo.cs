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
    
    public DrawState? CurrentState => Child.CurrentState?.Shift(Left, Top);

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
    
    internal DrawState TransformState()
    {
        var ordered = _underlying
            .Where(ch => ch.CurrentState is not null)
            .Select(ch => ch.CurrentState!)
            .Append(CurrentState!)
            .Concat(
                _overlapping
                    .Where(ch => ch.CurrentState is not null)
                    .Select(ch => ch.CurrentState!))
            .ToArray();
        
        return DrawState.Combine(ordered)
            .Crop(Left, Top, Width, Height);
    }
    
    public bool RemoveIfOverlapping(ChildInfo childInfo)
    {
        return _overlapping.Remove(childInfo) && childInfo._underlying.Remove(this) || 
               childInfo._overlapping.Remove(this) && _underlying.Remove(childInfo);
    }

    public DrawState CreateErasingState()
    {
        // Creating state of all pixels just default-backgrounded.
        DrawState notVisibleState = new(
            Enumerable.Range(0, Height)
                .Select(i => new PixelLine(Left, Top + i,
                    Enumerable.Range(0, Width)
                        .Select(_ => new PixelInfo())
                        .ToArray()))
                .ToArray()
        );
        
        var ordered = _underlying
            .Where(ch => ch.CurrentState is not null)
            .Select(ch => ch.CurrentState!)
            .Append(notVisibleState)
            .Concat(
                _overlapping
                    .Where(ch => ch.CurrentState is not null)
                    .Select(ch => ch.CurrentState!))
            .ToArray();
        
        return DrawState.Combine(ordered)
            .Crop(Left, Top, Width, Height);
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