using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public sealed class GridRowDefinition : IReadOnlyList<GridRow>
{
    private readonly IReadOnlyList<GridRow> _rows;

    public IEnumerator<GridRow> GetEnumerator() => _rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_rows).GetEnumerator();

    public int Count => _rows.Count;

    public GridRow this[int index] => _rows[index];

    private void ValidateRows(GridRow[] rows)
    {
        if (!rows.Any(row => row.IsRowRelation))
        {
            throw new GridDefinitionException("At least one grid row must be of RowRelation.");
        }
    }
    
    public static GridRowDefinition From(IEnumerable<GridRow> rows)
    {
        var rowsArr = rows.ToArray();
        
        return new GridRowDefinition(rowsArr);
    }

    public static GridRowDefinition From(params double[] rowRelations)
    {
        return rowRelations
            .Select(GridRow.FromRowRelation)
            .ToDefinition();
    }
    
    private GridRowDefinition(GridRow[] rows)
    {
        ValidateRows(rows);
        _rows = rows;
    }
}

internal static partial class CollectionExtensions
{
    public static GridRowDefinition ToDefinition(this IEnumerable<GridRow> rows)
    {
        return GridRowDefinition.From(rows);
    }
}