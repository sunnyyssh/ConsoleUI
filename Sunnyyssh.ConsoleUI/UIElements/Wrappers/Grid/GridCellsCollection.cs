﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public record GridCell(ChildInfo ChildInfo, int Column, int Row);

public sealed class GridCellsCollection : IReadOnlyList<GridCell>
{
    private readonly IReadOnlyList<GridCell> _gridCells;

    public IEnumerator<GridCell> GetEnumerator() => _gridCells.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_gridCells).GetEnumerator();

    public int Count => _gridCells.Count;

    GridCell IReadOnlyList<GridCell>.this[int index] => _gridCells[index];

    public GridCell? this[int column, int row] =>
        TryGet(column, row, out var result) ? result : null;

    public bool TryGet(int column, int row, [NotNullWhen(true)] out GridCell? cell)
    {
        cell = _gridCells.SingleOrDefault(cell => cell.Column == column && cell.Row == row);

        return cell is not null;
    }

    public ChildrenCollection ToChildrenCollection()
    {
        return this.Select(cell => cell.ChildInfo).ToCollection();
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
                    && cells[i].Column == cells[j].Column)
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

public static partial class CollectionExtensions
{
    public static GridCellsCollection ToCollection(this IEnumerable<GridCell> gridCells)
    {
        return GridCellsCollection.From(gridCells);
    }
}