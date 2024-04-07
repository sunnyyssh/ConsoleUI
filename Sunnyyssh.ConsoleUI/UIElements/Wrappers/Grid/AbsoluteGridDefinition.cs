namespace Sunnyyssh.ConsoleUI;

public record AbsoluteGridRow(int Width);

public record AbsoluteGridColumn(int Width);

public sealed class AbsoluteGridDefinition
{
    public IReadOnlyList<AbsoluteGridColumn> Columns { get; }

    public int ColumnCount => Columns.Count; 
    public IReadOnlyList<AbsoluteGridRow> Rows { get; }

    public int RowCount => Rows.Count;

    public AbsoluteGridDefinition(AbsoluteGridColumn[] columns, AbsoluteGridRow[] rows)
    {
        ArgumentNullException.ThrowIfNull(columns, nameof(columns));
        ArgumentNullException.ThrowIfNull(rows, nameof(rows));
        
        Columns = columns;
        Rows = rows;
    }
}