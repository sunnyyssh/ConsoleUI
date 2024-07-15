// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed record GridCell(ChildInfo ChildInfo, int Row, int Column);

public sealed class GridCellsCollection : IReadOnlyList<GridCell>
{
    private readonly IReadOnlyList<GridCell> _gridCells;

    public IEnumerator<GridCell> GetEnumerator() => _gridCells.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_gridCells).GetEnumerator();

    public int Count => _gridCells.Count;

    GridCell IReadOnlyList<GridCell>.this[int index] => _gridCells[index];

    public GridCell? this[int row, int column] =>
        TryGet(row, column, out var result) ? result : null;

    public bool TryGet(int row, int column, [NotNullWhen(true)] out GridCell? cell)
    {
        cell = _gridCells.SingleOrDefault(cell => cell.Column == column && cell.Row == row);

        return cell is not null;
    }

    public ImmutableList<ChildInfo> ToChildrenCollection()
    {
        return this.Select(cell => cell.ChildInfo).ToImmutableList();
    }

    public static GridCellsCollection From(IEnumerable<GridCell> gridCells)
    {
        var gridCellsArr = gridCells
            .OrderBy(cell => cell.Row)
            .ThenBy(cell => cell.Column)
            .ToArray();

        return new GridCellsCollection(gridCellsArr);
    }

    private void ValidateCells(GridCell[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            for (int j = i + 1; j < cells.Length; j++)
            {
                if (cells[i].Column == cells[j].Column
                    && cells[i].Row == cells[j].Row)
                {
                    throw new ChildPlacementException("Trying to add two children in one cell.");
                }

                if (cells[i].ChildInfo.Child == cells[j].ChildInfo.Child)
                {
                    throw new ChildPlacementException("Attempt to add two equal children occured.");
                }
            }
        }
    }
    
    private GridCellsCollection(GridCell[] cells)
    {
        ValidateCells(cells);
        
        _gridCells = cells;
    }
}

internal static partial class CollectionExtensions
{
    public static GridCellsCollection ToCollection(this IEnumerable<GridCell> gridCells)
    {
        return GridCellsCollection.From(gridCells);
    }
}