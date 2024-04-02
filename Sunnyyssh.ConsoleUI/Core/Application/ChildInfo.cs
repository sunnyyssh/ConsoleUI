using System.Collections;
using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents a collection of children that overlap given instance.
/// </summary>
internal sealed class OrderedOverlappingCollection : IEnumerable<ChildInfo>
{
    private readonly List<ChildInfo> _children = new();

    public bool Contains(ChildInfo childInfo) => _children.Contains(childInfo);

    public bool Add(ChildInfo childInfo)
    {
        if (Contains(childInfo))
            return false;
        int lastLowerPriorityIndex = _children
            .FindLastIndex(ch => ch.Child.Priority <= childInfo.Child.Priority);
        // Inserting after all children with lower or equal priority.
        // If there are no such children index will be -1 so it will be inserted at 0 position as expected.
        _children.Insert(lastLowerPriorityIndex + 1, childInfo);
        return true;
    }
    
    public IEnumerator<ChildInfo> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_children).GetEnumerator();
}

/// <summary>
/// Represents information of <see cref="UIElement"/> child and its position.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class ChildInfo
{
    private readonly OrderedOverlappingCollection _overlapping = new();
    
    private readonly OrderedOverlappingCollection _underlying = new();
    
    private string DebuggerDisplay => $"{Child}: Left={Left}; Top={Top}; Width={Width}; Height={Height}";
    
    /// <summary>
    /// <see cref="UIElement"/> instance of a child.
    /// </summary>
    public UIElement Child { get; }

    internal DrawState? CurrentState => Child.CurrentState?.Shift(Left, Top);
    
    /// <summary>
    /// Left absolute position (counted in characters).
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Top absolute position (counted in characters).
    /// </summary>
    public int Top { get; }

    /// <summary>
    /// The width of child.
    /// </summary>
    public int Width => Child.Width;

    /// <summary>
    /// The height of child.
    /// </summary>
    public int Height => Child.Height;

    /// <summary>
    /// Indicates if child implements IFocusable.
    /// </summary>
    public bool IsFocusable { get; }

    /// <summary>
    /// Indicates if given child is intersected with this instance.
    /// </summary>
    /// <param name="child">Child to check.</param>
    /// <returns>True if intersected, False otherwise.</returns>
    public bool IsIntersectedWith(ChildInfo child)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        
        bool horizontalIntersected = child.Left < Left + Width && child.Left + child.Width > Left;
        bool verticalIntersected = child.Top < Top + Height && child.Top + child.Height > Top;
        return horizontalIntersected && verticalIntersected;
    }

    /// <summary>
    /// Adds <see cref="ChildInfo"/> instance in collections of overlapping.
    /// </summary>
    /// <param name="possibleOverlapping"><see cref="ChildInfo"/> instance to check overlapping with.
    /// If <see cref="equalPriorityOverlapping"/> is true then this instance is overlapping.</param>
    /// <param name="equalPriorityOverlapping">If true this instance is overlapping.</param>
    /// <returns>True if successfully added. False otherwise</returns>
    internal bool AddIfOverlapping(ChildInfo possibleOverlapping, bool equalPriorityOverlapping)
    {
        if (possibleOverlapping == this)
            return false;
        if (_overlapping.Contains(possibleOverlapping))
            return false;
        if (IsIntersectedWith(possibleOverlapping))
        {
            if (Child.Priority < possibleOverlapping.Child.Priority
                || equalPriorityOverlapping && Child.Priority == possibleOverlapping.Child.Priority)
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

    /// <summary>
    /// Gets <see cref="DrawState"/> handled with overlapping and shifted to position.
    /// </summary>
    /// <returns>Transformed state.</returns>
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

    /// <summary>
    /// Creates <see cref="DrawState"/> that erases this child.
    /// </summary>
    /// <returns>Erasing state.</returns>
    internal DrawState CreateErasingState()
    {
        // Creating state of all pixels just default-backgrounded.
        var notVisibleState = new DrawStateBuilder(Width, Height)
            .Fill(new PixelInfo())
            .ToDrawState();
        
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

    /// <summary>
    /// Creates an instance of <see cref="ChildInfo"/> at given position.
    /// </summary>
    /// <param name="child"><see cref="UIElement"/> instance of child.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ChildInfo(UIElement child, int left, int top)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));

        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), left, null);
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), top, null);
        
        Child = child;
        Left = left;
        Top = top;
        IsFocusable = child is IFocusable;
    }
}