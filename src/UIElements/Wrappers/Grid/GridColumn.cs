// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class GridColumn
{
    public int? AbsoluteWidth { get; }

    [MemberNotNullWhen(true, nameof(AbsoluteWidth))]
    public bool IsAbsoluteWidth { get; }
    
    public double? RelationalWidth { get; }

    [MemberNotNullWhen(true, nameof(RelationalWidth))]
    public bool IsRelationalWidth { get; }
    
    public double? ColumnRelation { get; }

    [MemberNotNullWhen(true, nameof(ColumnRelation))]
    public bool IsColumnRelation { get; }

    public static GridColumn FromWidth(int absoluteWidth)
    {
        if (absoluteWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(absoluteWidth), absoluteWidth, null);
        
        return new GridColumn(absoluteWidth, null, null);
    }
    
    public static GridColumn FromWidth(double relationalWidth)
    {
        if (relationalWidth <= 0 || relationalWidth > 1)
            throw new ArgumentOutOfRangeException(nameof(relationalWidth), relationalWidth, null);
        
        return new GridColumn(null, relationalWidth, null);
    }

    public static GridColumn FromColumnRelation(double columnRelation)
    {
        if (columnRelation <= 0)
            throw new ArgumentOutOfRangeException(nameof(columnRelation), columnRelation, null);
        return new GridColumn(null, null, columnRelation);
    }
    
    private GridColumn(int? absoluteWidth, double? relationalWidth, double? columnRelation)
    {
        IsAbsoluteWidth = absoluteWidth is not null;
        AbsoluteWidth = absoluteWidth;

        IsRelationalWidth = relationalWidth is not null;
        RelationalWidth = relationalWidth;

        IsColumnRelation = columnRelation is not null;
        ColumnRelation = columnRelation;
    }
}