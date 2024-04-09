using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class GridRow
{
    public int? AbsoluteHeight { get; }

    [MemberNotNullWhen(true, nameof(AbsoluteHeight))]
    public bool IsAbsoluteHeight { get; }
    
    public double? RelationalHeight { get; }

    [MemberNotNullWhen(true, nameof(RelationalHeight))]
    public bool IsRelationalHeight { get; }
    
    public double? RowRelation { get; }

    [MemberNotNullWhen(true, nameof(RowRelation))]
    public bool IsRowRelation { get; }

    public static GridRow FromHeight(int absoluteHeight)
    {
        if (absoluteHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(absoluteHeight), absoluteHeight, null);
        
        return new GridRow(absoluteHeight, null, null);
    }
    
    public static GridRow FromHeight(double relationalHeight)
    {
        if (relationalHeight <= 0 || relationalHeight > 1)
            throw new ArgumentOutOfRangeException(nameof(relationalHeight), relationalHeight, null);
        
        return new GridRow(null, relationalHeight, null);
    }

    public static GridRow FromRowRelation(double rowRelation)
    {
        if (rowRelation <= 0)
            throw new ArgumentOutOfRangeException(nameof(rowRelation), rowRelation, null);
        return new GridRow(null, null, rowRelation);
    }
    
    private GridRow(int? absoluteHeight, double? relationalHeight, double? rowRelation)
    {
        IsAbsoluteHeight = absoluteHeight is not null;
        AbsoluteHeight = absoluteHeight;

        IsRelationalHeight = relationalHeight is not null;
        RelationalHeight = relationalHeight;

        IsRowRelation = rowRelation is not null;
        RowRelation = rowRelation;
    }
}