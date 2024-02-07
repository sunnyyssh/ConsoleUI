using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Sunnyyssh.ConsoleUI;


internal class ElementsField
{
    public int Width { get; private init; }
    
    public int Height { get; private init; }

    private readonly bool _allowOverlapping; 

    private readonly Dictionary<UIElement, ChildInfo> _children = new();

    public int ChildrenCount => _children.Count;

    public bool IsEmpty => !_children.Any();

    public ChildInfo[] GetChildInfos() => _children.Values.ToArray();

    public UIElement[] GetChildren() => _children.Keys.ToArray();
    
    public ElementsField(int width, int height, bool allowOverlapping)
    {
        Height = height;
        Width = width;
        _allowOverlapping = allowOverlapping;
    }

    public bool TryRemoveChild(UIElement child)
    {
        return _children.Remove(child);
    }

    public bool TryPlaceChild(UIElement child, Position position, [NotNullWhen(true)] out ChildInfo? childInfo)
    {
        // Trying to place and 
        bool placedSuccessfully = TryFindPlace(child, position, out childInfo);
        if (!placedSuccessfully)
        {
            if (!_allowOverlapping)
            {
                throw new ElementsPlacementException(
                    $"{child} is overlapping other UIElement instances. But overlapping is not allowed");
            }

            throw new NotImplementedException("How to overlap them?");
            return false;
        }

        bool addedSuccessfully = _children.TryAdd(child, childInfo);
        return addedSuccessfully;
    }

    private bool TryFindPlace(UIElement child, Position position, out ChildInfo childInfo)
    {
        // It should place children greedy.
        // It means if there is a size ambiguity then size should be as more as possible.
        throw new NotImplementedException();
    }
}