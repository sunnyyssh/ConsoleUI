// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public interface IDataTable<out TData>
{
    TData this[int column, int row] { get; }
    
    int RowCount { get; }
    
    int ColumnCount { get; }

    public TData[][] ToRowArray()
    {
        var result = new TData[RowCount][];
        
        for (int row = 0; row < RowCount; row++)
        {
            result[row] = new TData[ColumnCount];
            for (int column = 0; column < ColumnCount; column++)
            {
                result[row][column] = this[row, column];
            }
        }

        return result;
    }
}