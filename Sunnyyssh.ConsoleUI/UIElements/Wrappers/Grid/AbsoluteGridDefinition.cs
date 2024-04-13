using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed record AbsoluteGridRow(int Height);

public sealed record AbsoluteGridColumn(int Width);

public sealed class AbsoluteGridDefinition
{
    public IReadOnlyList<AbsoluteGridColumn> Columns { get; }

    public int ColumnCount => Columns.Count; 
    public IReadOnlyList<AbsoluteGridRow> Rows { get; }

    public int RowCount => Rows.Count;

    public AbsoluteGridDefinition(ImmutableList<AbsoluteGridColumn> columns, ImmutableList<AbsoluteGridRow> rows)
    {
        ArgumentNullException.ThrowIfNull(columns, nameof(columns));
        ArgumentNullException.ThrowIfNull(rows, nameof(rows));
        
        Columns = columns;
        Rows = rows;
    }
}