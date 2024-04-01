using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class Grid : Wrapper
{
    public LineComposition? Border { get; }
    
    public GridCellsCollection Cells { get; }

    [MemberNotNullWhen(true, nameof(Border))]
    public bool HasBorder { get; }

    private static ChildrenCollection ResolveChildren(GridCellsCollection cells, LineComposition? border)
    {
        var children = cells.Select(cell => cell.ChildInfo);

        if (border is not null)
        {
            children = children.Append(new ChildInfo(border, 0, 0));
        }

        return children.ToCollection();
    }
    
    internal Grid(int width, int height, LineComposition? border,
        AbsoluteGridDefinition absoluteDefinition, GridCellsCollection cells, 
        FocusFlowSpecification focusFlowSpecification, OverlappingPriority overlappingPriority) 
        : base(width, height, ResolveChildren(cells, border), focusFlowSpecification, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(absoluteDefinition, nameof(absoluteDefinition));

        Border = border;
        Cells = cells;

        HasBorder = border is not null;
    }
}