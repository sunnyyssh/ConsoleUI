using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public class DataTableUpdateArgs<TData> : UpdatedEventArgs
{
    public int Row { get; }
    
    public int Column { get; }
    
    public TData NewValue { get; }

    public DataTableUpdateArgs(int row, int column, TData newValue, string propertyName) 
        : base(propertyName)
    {
        Row = row;
        Column = column;
        NewValue = newValue;
    }
}

public class ObservableDataTable<TData> : IObservable<IDataTable<TData>, DataTableUpdateArgs<TData>>, IDataTable<TData>
{
    private readonly TData[,] _data;

    public IDataTable<TData> Value => this;
    
    public event UpdatedEventHandler<IDataTable<TData>, DataTableUpdateArgs<TData>>? Updated;

    public TData this[int column, int row]
    {
        get => _data[column, row];
        set
        {
            _data[column, row] = value;
            
            Updated?.Invoke(this, 
                new DataTableUpdateArgs<TData>(row, column, value, "data"));
        }
    }
    
    public int RowCount { get; }
    
    public int ColumnCount { get; }
    
    public IReadOnlyList<string> Headers { get; }

    public ObservableDataTable(int rowCount, string[] headers)
    {
        Headers = headers.ToArray();
        RowCount = rowCount;
        ColumnCount = Headers.Count;
        _data = new TData[ColumnCount, rowCount];
    }
}