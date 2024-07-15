// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Helps place children at specified area.
/// </summary>
/// <example>
/// <code>
/// var placementBuilder = new ElementsFieldBuilder(initWidth, initHeight, true);
///        
/// foreach (var queuedChild in _orderedQueuedChildren)
/// {
///    placementBuilder.Place(queuedChild.Builder, queuedChild.Position);
/// }
///
/// ChildrenCollection orderedChildren = placementBuilder.Build();
/// </code>
/// </example>
public sealed class ElementsFieldBuilder
{
    /// <summary>
    /// The width of area.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// The height of area.
    /// </summary>
    public int Height { get; }

    private readonly bool _enableOverlapping; 

    private readonly List<ChildInfo> _orderedChildren = new();

    /// <summary>
    /// Gets collection of placed children.
    /// </summary>
    /// <returns>Created collection of placed children.</returns>
    public ImmutableList<ChildInfo> Build() => _orderedChildren.ToImmutableList();
    
    /// <summary>
    /// Places <see cref="childBuilder"/> at specified position.
    /// </summary>
    /// <param name="childBuilder"><see cref="IUIElementBuilder"/> to add.</param>
    /// <param name="position">Position to place child at.</param>
    /// <returns>Same instance of <see cref="ElementsFieldBuilder"/>.</returns>
    public ElementsFieldBuilder Place(IUIElementBuilder childBuilder, Position position)
    {
        ArgumentNullException.ThrowIfNull(childBuilder, nameof(childBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        if (!TryPlace(childBuilder, position, out _))
        {
            throw new ChildPlacementException($"Cannot place child at this position. Child's builder was {childBuilder}");
        }
        
        return this;
    }
    
    /// <summary>
    /// Places <see cref="childBuilder"/> at specified position.
    /// </summary>
    /// <param name="childBuilder"><see cref="IUIElementBuilder"/> to add.</param>
    /// <param name="position">Position to place child at.</param>
    /// <param name="result">Placed <see cref="ChildInfo"/> instance.</param>
    /// <returns>Same <see cref="ElementsFieldBuilder"/> instance.</returns>
    /// <exception cref="ChildPlacementException">Couldn't place child.</exception>
    public ElementsFieldBuilder Place(IUIElementBuilder childBuilder, Position position, out ChildInfo result)
    {
        ArgumentNullException.ThrowIfNull(childBuilder, nameof(childBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        if (!TryPlace(childBuilder, position, out var resultChild))
        {
            throw new ChildPlacementException($"Cannot place child at this position. Child's builder was {childBuilder}");
        }

        result = resultChild;
        return this;
    }

    /// <summary>
    /// Tries to place child at specified position.
    /// </summary>
    /// <param name="childBuilder"><see cref="IUIElementBuilder"/> to add.</param>
    /// <param name="position">Position to place child at.</param>
    /// <param name="result">Placed <see cref="ChildInfo"/> instance.</param>
    /// <returns>True if successfully placed. False otherwise.</returns>
    public bool TryPlace(IUIElementBuilder childBuilder, Position position, 
        [NotNullWhen(true)] out ChildInfo? result)
    {
        ArgumentNullException.ThrowIfNull(childBuilder, nameof(childBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
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

        // Check if built child already exists.
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

    // Finds most suitable placement if it's possible.
    internal bool TryFindPlace(Size size, Position position, out bool intersected, 
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
            // Checking if right bottom bound is not so far.
            .Where(placement => NoRightBottomViolation(placement, size, pos))
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

    private bool NoRightBottomViolation(Placement placement, Size size, Position position)
    {
        double expectedBottom = (position.IsTopRelational ? position.TopRelational.Value * Height : position.Top.Value)
                                + (size.IsHeightRelational ? size.HeightRelation.Value * Height : size.Height.Value);
        
        double expectedRight = (position.IsLeftRelational ? position.LeftRelational.Value * Width : position.Left.Value)
                                + (size.IsWidthRelational ? size.WidthRelation.Value * Width : size.Width.Value);

        bool noBottomViolation = Math.Abs(expectedBottom - placement.Top - placement.Height) < 1;
        bool noRightViolation = Math.Abs(expectedRight - placement.Left - placement.Width) < 1;

        return noBottomViolation && noRightViolation;
    }

    private IEnumerable<int> RoundingVariants(bool takeRelational, int? absolute, double? relational, int absoluteMultiplier)
    {
        if (!takeRelational)
        {
            yield return absolute!.Value;
            yield break;
        }

        // Experimental !!!
        // It's used in order to make bounds more flexible if they are too solid.
        const double epsilon = 0.001;

        double raw = relational!.Value * absoluteMultiplier;
        int floor = Convert.ToInt32(Math.Floor(raw));
        int ceiling = Convert.ToInt32(Math.Ceiling(raw));
        int epsFloor = Convert.ToInt32(Math.Floor(raw - epsilon));
        int epsCeiling = Convert.ToInt32(Math.Ceiling(raw + epsilon));
        
        yield return floor;
        
        if (ceiling != floor)
            yield return ceiling;

        if (epsFloor != floor)
            yield return epsFloor;

        if (epsCeiling != ceiling)
            yield return epsCeiling;
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

    /// <summary>
    /// Creates an instance of <see cref="ElementsFieldBuilder"/>.
    /// </summary>
    /// <param name="width">The width of placement area.</param>
    /// <param name="height">The height of placement area.</param>
    /// <param name="enableOverlapping">If True then children can overlap each other. False otherwise.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ElementsFieldBuilder(int width, int height, bool enableOverlapping)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, null);
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, null);
        
        Width = width;
        Height = height;
        _enableOverlapping = enableOverlapping;
    }
}