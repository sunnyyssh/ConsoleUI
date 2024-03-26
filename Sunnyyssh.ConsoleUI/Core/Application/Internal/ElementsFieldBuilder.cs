using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class ElementsFieldBuilder
{
    public int Width { get; }
    
    public int Height { get; }

    private readonly bool _enableOverlapping; 

    private readonly List<ChildInfo> _orderedChildren = new();

    public ChildInfo[] Build() => _orderedChildren.ToArray();
    
    public ElementsFieldBuilder Place(IUIElementBuilder childBuilder, Position position)
    {
        if (!TryPlace(childBuilder, position, out _))
        {
            throw new ChildPlacementException("Cannot place child at this position.");
        }
        
        return this;
    }
    
    public ElementsFieldBuilder Place(UIElement child, Position position)
    {
        if (!TryPlace(child, position, out _))
        {
            throw new ChildPlacementException("Cannot place child at this position.");
        }

        return this;
    }
    
    public ElementsFieldBuilder Place(IUIElementBuilder childBuilder, Position position, out ChildInfo result)
    {
        if (!TryPlace(childBuilder, position, out var resultChild))
        {
            throw new ChildPlacementException("Cannot place child at this position.");
        }

        result = resultChild;
        return this;
    }
    
    public ElementsFieldBuilder Place(UIElement child, Position position, out ChildInfo result)
    {
        if (!TryPlace(child, position, out var resultChild))
        {
            throw new ChildPlacementException("Cannot place child at this position.");
        }

        result = resultChild;
        return this;
    }

    public bool TryPlace(IUIElementBuilder childBuilder, Position position, 
        [NotNullWhen(true)] out ChildInfo? result)
    {
        result = null;
        
        if (!TryFindPlace(childBuilder.Size, position, out bool intersected, out var placement))
        {
            return false;
        }
        
        if (intersected && !_enableOverlapping)
        {
            return false;
        }

        var builderArgs = new UIElementBuildArgs(placement.Width, placement.Height);

        var builtChild = childBuilder.Build(builderArgs);
        
        var created = new ChildInfo(builtChild, placement.Left, placement.Top);

        if (_orderedChildren.Any(ch => ch.Child == created.Child))
        {
            return false;
        }
        _orderedChildren.Add(created);
        
        foreach (var item in _orderedChildren)
        {
            created.AddIfOverlapping(item, false);
            item.AddIfOverlapping(created, true);
        }

        result = created;
        return true;
    }
    
    public bool TryPlace(UIElement child, Position position, 
        [NotNullWhen(true)] out ChildInfo? result)
    {
        result = null;
        
        if (_orderedChildren.Any(ch => ch.Child == child) || IsChildContained(child))
        {
            return false;
        }

        var childSize = new Size(child.Width, child.Height);

        if (!TryFindPlace(childSize, position, out bool intersected, out var placement))
        {
            return false;
        }
        
        if (intersected && !_enableOverlapping)
        {
            return false;
        }

        var created = new ChildInfo(child, placement.Left, placement.Top);

        if (_orderedChildren.Any(ch => ch.Child == created.Child))
        {
            return false;
        }
        _orderedChildren.Add(created);
        
        foreach (var item in _orderedChildren)
        {
            created.AddIfOverlapping(item, false);
            item.AddIfOverlapping(created, true);
        }

        result = created;
        return true;
    }

    private bool IsChildContained(UIElement element)
    {
        return _orderedChildren.Any(ch =>
            ch.Child is IElementContainer container
            && container.Contains(element));
    }

    // Finds most suitable placement if it's possible.
    private bool TryFindPlace(Size size, Position position, out bool intersected, 
        [NotNullWhen(true)] out Placement? result)
    {
        // It should place children greedy.
        // It means if there is a size ambiguity then size should be as more as possible.
        // It should take the leftest the topest position and the biggest size. 

        var placementModel = CreateChildPlacementModel(position, size);

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
        var (placement, (intersections, _)) = placementModel
            .OrderBy(modelInfo => 
                modelInfo.Value.intersections)
            .ThenByDescending(modelInfo => 
                modelInfo.Key.Height + modelInfo.Key.Width)
            .ThenByDescending(modelInfo => 
                modelInfo.Value.positiveTouches)
            .ThenBy(modelInfo => 
                modelInfo.Key.Left + modelInfo.Key.Top)
            .First();

        result = placement;
        intersected = intersections > 0;
        return true;
    }

    // Returning dictionary description:
    // Different placements are possible because of ambigious rounding Relational * Absolute multiplication.
    // And we can round Relational * Absolute either up or down.
    // The dictionary's keys are placemenets (left, top, width, height)
    // The dictionary's values are the count of the intersections with the other children by specific position
    // and the count of the touches with the other children 

    private Dictionary<Placement, (int intersections, int positiveTouches)> CreateChildPlacementModel(Position pos, Size size)
    {
        // Other boxes which to count intersetions with.
        var boxes = _orderedChildren
            .Select(childInfo => new Placement(childInfo.Left, childInfo.Top, childInfo.Width, childInfo.Height))
            .ToArray();

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
                return new Placement(left, top, width, height);
            })
            // Removing placement that are not inside the field.
            .Where(IsPlacementInsideField)
            // Turning combinations into the resulting dictionary 
            .ToDictionary(
                // ectracting Code of placement (it's {0, 1}^4 as described higher)
                keySelector: placement => placement,
                elementSelector: placement =>
                {
                    int intersections = CountIntersections(boxes, placement);
                    int positiveTouches = CountPositiveTouches(boxes, placement);
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
    
    private IEnumerable<int> RoundingVariants(bool takeRelational, int? absolute, double? relational, int absoluteMultiplier)
    {
        if (!takeRelational)
        {
            yield return absolute!.Value;
            yield break;
        }

        double raw = relational!.Value * absoluteMultiplier;
        int floor = Convert.ToInt32(Math.Floor(raw));
        int ceiling = Convert.ToInt32(Math.Ceiling(raw));
        
        yield return floor;
        
        if (ceiling != floor)
            yield return ceiling;
    }

    private bool IsPlacementInsideField(Placement counting)
    {
        if (counting.Left < 0)
            return false;
        if (counting.Top < 0)
            return false;
        if (counting.Left + counting.Width > Width)
            return false;
        if (counting.Top + counting.Height > Height)
            return false;
        
        return true;
    }

    private int CountIntersections(Placement[] boxes, Placement counting)
    {
        return boxes.Count(box => Placement.AreIntersected(box, counting));
    }

    private int CountPositiveTouches(Placement[] boxes, Placement counting)
    {
        return boxes.Count(box => Placement.AreTouched(box, counting));
    }

    public ElementsFieldBuilder(int width, int height, bool enableOverlapping)
    {
        Width = width;
        Height = height;
        _enableOverlapping = enableOverlapping;
    }
}