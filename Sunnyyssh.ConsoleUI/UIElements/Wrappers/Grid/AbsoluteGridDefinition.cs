namespace Sunnyyssh.ConsoleUI;

internal record AbsoluteGridRow(int Width);

internal record AbsoluteGridColumn(int Width);

internal sealed class AbsoluteGridDefinition
{
    public IReadOnlyList<AbsoluteGridColumn> Columns { get; }
    public IReadOnlyList<AbsoluteGridRow> Rows { get; }

    public AbsoluteGridDefinition(AbsoluteGridColumn[] columns, AbsoluteGridRow[] rows)
    {
        Columns = columns;
        Rows = rows;
    }
}