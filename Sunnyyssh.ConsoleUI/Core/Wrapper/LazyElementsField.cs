using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal sealed class LazyElementsField
{
    private readonly bool _allowOverlapping;

    [MemberNotNullWhen(true, nameof(Field))]
    public bool IsInitialized => Field is not null;

    private ConcurrentDictionary<UIElement, Position> _enqueuedChildren = new();

    public UIElement[] GetEnqueuedChildren() => _enqueuedChildren.Keys.ToArray();

    [MemberNotNull(nameof(Field))]
    public void Initialize(int width, int height)
    {
        Field = new ElementsField(width, height, _allowOverlapping);
        foreach (var (child, position) in _enqueuedChildren)
        {
            bool placementResult = Field.TryPlaceChild(child, position, out _);
            
            if (!placementResult)
                throw new ChildPlacementException($"Child {child} can't be placed.");
        }

        _enqueuedChildren = new ConcurrentDictionary<UIElement, Position>();
    }
    
    public ElementsField? Field { get; private set; }

    public bool ContainsEnqueuedChild(UIElement child) 
        => _enqueuedChildren.ContainsKey(child);

    public bool EnqueuePlaceChild(UIElement child, Position position)
    {
        if (IsInitialized)
        {
            return false;
        }

        return _enqueuedChildren.TryAdd(child, position);
    }

    public bool RemoveEnqueued(UIElement child)
    {
        return _enqueuedChildren.TryRemove(child, out _);
    }

    public LazyElementsField(bool allowOverlapping)
    {
        _allowOverlapping = allowOverlapping;
    }
}