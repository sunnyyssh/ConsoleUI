// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections;
using Sunnyyssh.ConsoleUI.Binding;

namespace Sunnyyssh.ConsoleUI;

public class DataTableUpdateArgs<TData> : UpdatedEventArgs
{
    public int Row { get; }
    
    public int Column { get; }
    
    public TData NewValue { get; }

    public DataTableUpdateArgs(int row, int column, TData newValue)
    {
        Row = row;
        Column = column;
        NewValue = newValue;
    }
}

public class BindableDataTable<TData> : IBindable<IDataTable<TData>, DataTableUpdateArgs<TData>>, IDataTable<TData>
{
    private readonly TData[,] _data;

    public IDataTable<TData> Value => this;
    
    public event UpdatedEventHandler<IDataTable<TData>, DataTableUpdateArgs<TData>>? Updated;

    public TData this[int row, int column]
    {
        get => _data[row, column];
        set
        {
            _data[row, column] = value;
            
            Updated?.Invoke(this, 
                new DataTableUpdateArgs<TData>(row, column, value));
        }
    }
    
    public int RowCount { get; }
    
    public int ColumnCount { get; }

    public void HandleUpdate(DataTableUpdateArgs<TData> args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        _data[args.Column, args.Row] = args.NewValue;
        BoundUpdated?.Invoke(this, args);
    }

    public event UpdatedEventHandler<IDataTable<TData>, DataTableUpdateArgs<TData>>? BoundUpdated;

    public BindableDataTable(int rowCount, int columnCount, TData initValue)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
        
        _data = new TData[RowCount, ColumnCount];
        
        for (int column = 0; column < ColumnCount; column++)
        {
            for (int row = 0; row < RowCount; row++)
            {
                _data[row, column] = initValue;
            }
        }
    }
}