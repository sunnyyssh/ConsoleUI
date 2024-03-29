using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public sealed class ChildrenCollection : IReadOnlyList<ChildInfo>
{
    public static readonly ChildrenCollection Empty = new(Array.Empty<ChildInfo>());
    
    private readonly IReadOnlyList<ChildInfo> _children;
    
    public IEnumerator<ChildInfo> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _children.Count;

    public ChildInfo this[int index] => throw new NotImplementedException();
    
    private void ValidateChildren(IReadOnlyList<ChildInfo> orderedChildren)
    {
        for (int i = 0; i < orderedChildren.Count; i++)
        {
            for (int j = i + 1; j < orderedChildren.Count; j++)
            {
                if (orderedChildren[i].Child == orderedChildren[j].Child)
                    throw new ChildPlacementException("Attempt to add two equal children occured.");
            }
        }
    }

    public static ChildrenCollection From(IEnumerable<ChildInfo> children)
    {
        var orderedChildren = children.ToArray();
        
        return new ChildrenCollection(orderedChildren);
    }

    private ChildrenCollection(ChildInfo[] orderedChildren)
    {
        ArgumentNullException.ThrowIfNull(orderedChildren, nameof(orderedChildren));

        ValidateChildren(orderedChildren);
        
        _children = orderedChildren;
    }
}

public static partial class CollectionExtensions
{
    public static ChildrenCollection ToCollection(this IEnumerable<ChildInfo> children)
    {
        return ChildrenCollection.From(children);
    }
}