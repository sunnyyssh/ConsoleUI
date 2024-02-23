using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal class ElementsField
{
    private readonly object _childrenCollectionLock = new();
    
    public int Width { get; private init; }
    
    public int Height { get; private init; }

    private readonly bool _allowOverlapping; 

    private readonly Dictionary<UIElement, ChildInfo> _children = new();

    public int ChildrenCount
    {
        get
        {
            lock (_childrenCollectionLock)
            {
                return _children.Count;
            }
        }
    }
    
    public bool IsEmpty 
    {
        get
        {
            lock (_childrenCollectionLock)
            {
                return !_children.Any();
            }
        }
    }

    public bool Contains(UIElement child)
    {
        lock (_childrenCollectionLock)
        {
            return _children.ContainsKey(child);
        }
    }

    public ChildInfo[] GetChildInfos()
    {
        lock (_childrenCollectionLock)
        {
            return _children.Values.ToArray();
        }
    }

    public UIElement[] GetChildren()
    {
        lock (_childrenCollectionLock)
        {
            return _children.Keys.ToArray();
        }
    }

    public ElementsField(int width, int height, bool allowOverlapping)
    {
        Height = height;
        Width = width;
        _allowOverlapping = allowOverlapping;
    }

    public bool TryRemoveChild(UIElement child)
    {
        lock (_childrenCollectionLock)
        {
            if (!_children.TryGetValue(child, out var childInfo))
                return false;
            foreach (var (_, item) in _children)
            {
                childInfo.RemoveIfOverlapping(item);
                item.RemoveIfOverlapping(childInfo);
            }
            return _children.Remove(child);
        }
    }

    public bool TryGetChild(UIElement element, [NotNullWhen(true)] out ChildInfo? result)
    {
        lock (_childrenCollectionLock)
        {
            return _children.TryGetValue(element, out result);
        }
    }

    public bool TryPlaceChild(UIElement child, Position position, [NotNullWhen(true)] out ChildInfo? childInfo)
    {
        lock (_childrenCollectionLock)
        {
            if (_children.ContainsKey(child) || IsChildContained(child))
            {
                childInfo = null;
                return false;
            }
            if (!TryFindMostSuitablePlace(child, position, out bool intersected, out childInfo))
                return false;
        
            if (intersected && !_allowOverlapping)
            {
                childInfo = null;
                return false;
            }
        
            bool addedSuccessfully = _children.TryAdd(child, childInfo);
        
            if (addedSuccessfully)
            {
                foreach (var (_, item) in _children)
                {
                    childInfo.AddIfOverlapping(item, false);
                    item.AddIfOverlapping(childInfo, true);
                }
            }
            else
            {
                childInfo = null;
            }
            
            return addedSuccessfully;
        }
    }

    private bool IsChildContained(UIElement element)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        foreach (var (child, _) in _children)
        {
            if (child is IElementContainer container)
            {
                if (container.Contains(element))
                    return true;
            }
        }

        return false;
    }
    
    private bool TryFindMostSuitablePlace(UIElement child, Position position, out bool intersected, [NotNullWhen(true)] out ChildInfo? result)
    {
        // It should place children greedy.
        // It means if there is a size ambiguity then size should be as more as possible.
        // It should take the leftest the topest position and the biggest size. 

        var placementModel = CreateChildPlacementModel(position, child.Size);

        if (!placementModel.Any())
        {
            intersected = false;
            result = null;
            return false;
        }
        
        // Finding a placement with: (in order of priority)
        // 1. min intersections
        // 2. max size
        // 3. max touches
        // 4. The leftmost and topmost position.
        var ((left, top, width, height), (intersections, _)) = placementModel
            .OrderBy(placement => placement.Value.intersections)
            .ThenByDescending(placement => placement.Key.height + placement.Key.width)
            .ThenByDescending(placement => placement.Value.positiveTouches)
            .ThenBy(placement => placement.Key.left + placement.Key.top)
            .First();

        result = new ChildInfo(child, left, top, width, height);
        intersected = intersections > 0;
        return true;
    }

    // Returning dictionary description:
    // Different placements are possible because of ambigious rounding Relational * Absolute multiplication.
    // And we can round Relational * Absolute either up or down.
    // The dictionary's keys are placemenets (left, top, width, height)
    // The dictionary's values are the count of the intersections with the other children by specific position
    // and the count of the touches with the other children 
    private Dictionary<(int left, int top, int width, int height), (int intersections, int positiveTouches)> 
        CreateChildPlacementModel(Position pos, Size size)
    {
        // Other boxes which to count intersetions with.
        var boxes = GetChildInfos();

        // Variants of each placemenet property.
        // It may have multiple variants because of ambigiouty of rounding the relational size convereted to absolute
        var leftVariants = RoundingVariants(pos.IsLeftRelational, pos.Left, pos.LeftRelational, Width);
        var topVariants = RoundingVariants(pos.IsTopRelational, pos.Top, pos.TopRelational, Height);
        var widthVariants = RoundingVariants(size.IsWidthRelational, size.Width, size.WidthRelation, Width);
        var heightVariants = RoundingVariants(size.IsHeightRelational, size.Height, size.HeightRelation, Height);

        var resultPlacements =
            // Geting all combinations of variants.
            GetAllCombinations(
                GetAllCombinations(
                    GetAllCombinations(
                        leftVariants,
                        topVariants),
                    widthVariants),
                heightVariants)
            // Combinations were gotten in tuple tree we have to unwrap it.
            .Select(bigTuple =>
            {
                var (((left, top), width), height) = bigTuple;
                return new
                {
                    left, top, width, height
                };
            })
            // Removing placement that are not inside the field.
            .Where(placement => IsPlacementInsideField(placement.left, placement.top, placement.width, placement.height))
            // Turning combinations into the resulting dictionary 
            .ToDictionary(
                // ectracting Code of placement (it's {0, 1}^4 as described higher)
                keySelector: placement =>
                    (placement.left, placement.top, placement.width, placement.height),
                elementSelector: p =>
                {
                    int intersections = CountIntersections(boxes, 
                        p.left, p.top, p.width, p.height);
                    int positiveTouches = CountPositiveTouches(boxes, 
                        p.left, p.top, p.width, p.height);
                    return (intersections, positiveTouches);
                }
            );
        
        return resultPlacements;

        IEnumerable<(T1, T2)> GetAllCombinations<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second)
        {
            var resultCombinations = first.SelectMany(
                _ => second,
                (value1, value2) => (value1, value2)
            );
            
            return resultCombinations;
        }
    }
    
    // The first integer is code 0 or 1 indicating the min or max value is taken.
    // The second one is the actual absolute value.
    private IEnumerable<int> RoundingVariants(bool takeRelational, int? absolute, double? relational, int absoluteMultiplier)
    {
        if (!takeRelational)
        {
            yield return absolute!.Value;
            yield break;
        }

        double rowMul = relational!.Value * absoluteMultiplier;
        int floor = Convert.ToInt32(Math.Floor(rowMul));
        int ceiling = Convert.ToInt32(Math.Ceiling(rowMul));
        yield return floor;
        if (ceiling != floor)
            yield return ceiling;
    }

    private bool IsPlacementInsideField(int left, int top, int width, int height)
    {
        if (left < 0)
            return false;
        if (top < 0)
            return false;
        if (left + width > Width)
            return false;
        if (top + height > Height)
            return false;
        
        return true;
    }

    private int CountIntersections(ChildInfo[] boxes, int left, int top, int width, int height)
    {
        return boxes.Count(IsIntersectedWith);
        
        bool IsIntersectedWith(ChildInfo box)
        {
            bool horizontalIntersected = box.Left < left + width && box.Left + box.Width > left;
            bool verticalIntersected = box.Top < top + height && box.Top + box.Height > top;
            return horizontalIntersected && verticalIntersected;
        }
    }

    private int CountPositiveTouches(ChildInfo[] boxes, int left, int top, int width, int height)
    {
        return boxes.Count(IsTouchedWith);
        
        bool IsTouchedWith(ChildInfo box)
        {
            bool horizontalIntersected = box.Left < left + width && box.Left + box.Width > left;
            bool verticalIntersected = box.Top < top + height && box.Top + box.Height > top;
            bool horizontalTouched = box.Top == top + height || box.Top + box.Height == top;
            bool verticalTouched = box.Left == left + width || box.Left + box.Width == left;
            return horizontalTouched && horizontalIntersected || verticalTouched && verticalIntersected;
        }
    }
    
}