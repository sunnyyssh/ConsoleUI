using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// <see cref="Wrapper"/> that can place <see cref="UIElement"/> in the specific grid cells.
/// </summary>
public sealed class Grid : CompositionWrapper
{
    public LineComposition? Border { get; }
    public AbsoluteGridDefinition AbsoluteDefinition { get; }

    public GridCellsCollection Cells { get; }

    [MemberNotNullWhen(true, nameof(Border))]
    public bool HasBorder { get; }

    private static ImmutableList<ChildInfo> ResolveCompositionChildren(GridCellsCollection cells, LineComposition? border)
    {
        var children = cells.Select(cell => cell.ChildInfo);

        if (border is not null)
        {
            children = children.Append(new ChildInfo(border, 0, 0));
        }

        return children.ToImmutableList();
    }

    private static ImmutableList<ChildInfo> ResolveChildren(GridCellsCollection cells)
    {
        return cells
            .Select(cell => cell.ChildInfo)
            .ToImmutableList();
    }
    
    internal Grid(int width, int height, LineComposition? border,
        AbsoluteGridDefinition absoluteDefinition, GridCellsCollection cells, 
        FocusFlowSpecification focusFlowSpecification, OverlappingPriority overlappingPriority) 
        : base(width, height, ResolveChildren(cells), ResolveCompositionChildren(cells, border),
            focusFlowSpecification, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(absoluteDefinition, nameof(absoluteDefinition));

        Border = border;
        AbsoluteDefinition = absoluteDefinition;
        Cells = cells;

        HasBorder = border is not null;
    }
}