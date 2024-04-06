namespace Sunnyyssh.ConsoleUI;

public interface IDataTable<TData>
{
    TData this[int column, int row] { get; }
    
    int RowCount { get; }
    
    int ColumnCount { get; }
    
    IReadOnlyList<string> Headers { get; }
}