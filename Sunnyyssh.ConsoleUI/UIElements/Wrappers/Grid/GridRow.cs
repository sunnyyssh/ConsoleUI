using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class GridRow
{
    public int? AbsoluteWidth { get; }

    [MemberNotNullWhen(true, nameof(AbsoluteWidth))]
    public bool IsAbsoluteWidth { get; }
    
    public double? RelationalWidth { get; }

    [MemberNotNullWhen(true, nameof(RelationalWidth))]
    public bool IsRelationalWidth { get; }
    
    public double? RowRelation { get; }

    [MemberNotNullWhen(true, nameof(RowRelation))]
    public bool IsRowRelation { get; }

    public static GridRow FromWidth(int absoluteWidth)
    {
        if (absoluteWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(absoluteWidth), absoluteWidth, null);
        
        return new GridRow(absoluteWidth, null, null);
    }
    
    public static GridRow FromWidth(double relationalWidth)
    {
        if (relationalWidth <= 0 || relationalWidth > 1)
            throw new ArgumentOutOfRangeException(nameof(relationalWidth), relationalWidth, null);
        
        return new GridRow(null, relationalWidth, null);
    }

    public static GridRow FromRowRelation(double rowRelation)
    {
        if (rowRelation <= 0)
            throw new ArgumentOutOfRangeException(nameof(rowRelation), rowRelation, null);
        return new GridRow(null, null, rowRelation);
    }
    
    private GridRow(int? absoluteWidth, double? relationalWidth, double? rowRelation)
    {
        IsAbsoluteWidth = absoluteWidth is not null;
        AbsoluteWidth = absoluteWidth;

        IsRelationalWidth = relationalWidth is not null;
        RelationalWidth = relationalWidth;

        IsRowRelation = rowRelation is not null;
        RowRelation = rowRelation;
    }
}