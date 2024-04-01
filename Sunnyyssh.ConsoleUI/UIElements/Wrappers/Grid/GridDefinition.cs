namespace Sunnyyssh.ConsoleUI;

public sealed class GridDefinition
{
    public GridColumnDefinition ColumnDefinition { get; }
    
    public GridRowDefinition RowDefinition { get; }

    public int ColumnCount => ColumnDefinition.Count;
    
    public int RowCount => RowDefinition.Count;

    public GridDefinition(GridColumnDefinition columnDefinition, GridRowDefinition rowDefinition)
    {
        RowDefinition = rowDefinition;
        ColumnDefinition = columnDefinition;
    }
}