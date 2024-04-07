using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public sealed class GridColumnDefinition : IReadOnlyList<GridColumn>
{
    private readonly IReadOnlyList<GridColumn> _columns;

    public IEnumerator<GridColumn> GetEnumerator() => _columns.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_columns).GetEnumerator();

    public int Count => _columns.Count;

    public GridColumn this[int index] => _columns[index];

    public static GridColumnDefinition From(IEnumerable<GridColumn> columns)
    {
        var columnsArr = columns.ToArray();

        return new GridColumnDefinition(columnsArr);
    }

    public static GridColumnDefinition From(params double[] columnRelations)
    {
        return columnRelations
            .Select(GridColumn.FromColumnRelation)
            .ToDefinition();
    }

    private void ValidateColumns(GridColumn[] columns)
    {
        if (!columns.Any(column => column.IsColumnRelation))
        {
            throw new GridDefinitionException("At least one grid column must be of ColumnRelation.");
        }
    }
    
    private GridColumnDefinition(GridColumn[] columns)
    {
        ValidateColumns(columns);
        _columns = columns;
    }
}

internal static partial class CollectionExtensions
{
    public static GridColumnDefinition ToDefinition(this IEnumerable<GridColumn> columns)
    {
        return GridColumnDefinition.From(columns);
    }
}