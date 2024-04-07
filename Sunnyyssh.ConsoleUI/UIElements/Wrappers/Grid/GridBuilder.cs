namespace Sunnyyssh.ConsoleUI;

public sealed class GridBuilder : IUIElementBuilder<Grid>
{
    private static readonly ConsoleKeyCollection DefaultFocusChangeKeys 
        = new[] { ConsoleKey.Tab }.ToCollection();
    
    private static readonly ConsoleKeyCollection DefaultFocusUpKeys 
        = new[] { ConsoleKey.UpArrow }.ToCollection();
    
    private static readonly ConsoleKeyCollection DefaultFocusDownKeys 
        = new[] { ConsoleKey.DownArrow }.ToCollection();
    
    private static readonly ConsoleKeyCollection DefaultFocusRightKeys 
        = new[] { ConsoleKey.RightArrow }.ToCollection();
    
    private static readonly ConsoleKeyCollection DefaultFocusLeftKeys 
        = new[] { ConsoleKey.LeftArrow }.ToCollection();
    
    private readonly QueuedPositionChild?[,] _queuedCellChildren;
    
    public Size Size { get; }

    public ConsoleKeyCollection? FocusChangeKeys { get; init; }

    public ConsoleKeyCollection? FocusUpKeys { get; init; }

    public ConsoleKeyCollection? FocusDownKeys { get; init; }

    public ConsoleKeyCollection? FocusRightKeys { get; init; }

    public ConsoleKeyCollection? FocusLeftKeys { get; init; }

    public bool FocusFlowLoop { get; init; } = false;

    public bool OverridesFocusFlow { get; init; } = true;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Color BorderColor { get; init; } = Color.Default;
    
    public LineCharSet? BorderLineCharSet { get; init; } = null;

    public BorderKind BorderKind { get; init; } = BorderKind.None;
    
    public GridDefinition Definition { get; }

    public GridBuilder Add(IUIElementBuilder builder, int column, int row)
    {
        return Add(builder, column, row, new Position(0, 0));
    }

    public GridBuilder Add(IUIElementBuilder builder, int column, int row, Position position)
    {
        if (column < 0 || column >= Definition.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(column), column, null);
        
        if (row < 0 || row >= Definition.RowCount)
            throw new ArgumentOutOfRangeException(nameof(row), row, null);

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        if (_queuedCellChildren[column, row] is not null)
            throw new ArgumentException("Grid cell already has child.");

        _queuedCellChildren[column, row] = new QueuedPositionChild(builder, position);

        return this;
    }

    public Grid Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        int width = args.Width;
        int height = args.Height;

        var borderCharSet = BorderLineCharSet ?? (BorderKind != BorderKind.None ? LineCharSets.Of(BorderKind) : null);
        
        bool hasBorders = borderCharSet is not null;

        var (gridCells, absoluteDefinition) = ResolveGrid(width, height, hasBorders);

        var focusFlowSpecification = CreateFocusSpecification(gridCells);

        var border = hasBorders ? CreateBorder(width, height, absoluteDefinition, borderCharSet!) : null;

        var resultGrid = new Grid(width, height, 
            border, absoluteDefinition, gridCells, 
            focusFlowSpecification, OverlappingPriority);

        return resultGrid;
    }

    private FocusFlowSpecification CreateFocusSpecification(GridCellsCollection allCells)
    {
        var changeKeys = FocusChangeKeys ?? DefaultFocusChangeKeys;
        var upKeys = FocusUpKeys ?? DefaultFocusUpKeys;
        var downKeys = FocusDownKeys ?? DefaultFocusDownKeys;
        var rightKeys = FocusRightKeys ?? DefaultFocusRightKeys;
        var leftKeys = FocusLeftKeys ?? DefaultFocusLeftKeys;
        
        var focusableCells = allCells
            .Where(cell => cell.ChildInfo.IsFocusable)
            .ToCollection();

        if (focusableCells.Count == 0)
        {
            return new FocusFlowSpecification(new Dictionary<IFocusable, ChildSpecification>(), false);
        }

        return CreateFocusSpecification(focusableCells, changeKeys, upKeys, downKeys, rightKeys, leftKeys);
    }

    private FocusFlowSpecification CreateFocusSpecification(GridCellsCollection focusableCells, ConsoleKeyCollection changeKeys, 
        ConsoleKeyCollection upKeys, ConsoleKeyCollection downKeys, 
        ConsoleKeyCollection rightKeys, ConsoleKeyCollection leftKeys)
    {
        var specBuilder = new FocusFlowSpecificationBuilder(OverridesFocusFlow);

        foreach (var (childInfo, _, _) in focusableCells)
        {
            // It's guaranteed that child is IFocusable.
            specBuilder.Add((IFocusable)childInfo.Child);
        }

        var linearFocusables = focusableCells
            .Select(cell => (IFocusable)cell.ChildInfo.Child)
            .ToArray();
        AddLinearFlow(linearFocusables, changeKeys, specBuilder);
        
        foreach (var (childInfo, column, row) in focusableCells)
        {
            var currentFocusable = (IFocusable)childInfo.Child;
            
            if (focusableCells.TryGet(column - 1, row, out var leftCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)leftCell.ChildInfo.Child, leftKeys);
            }
            
            if (focusableCells.TryGet(column + 1, row, out var rightCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)rightCell.ChildInfo.Child, rightKeys);
            }
            
            if (focusableCells.TryGet(column, row - 1, out var upCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)upCell.ChildInfo.Child, upKeys);
            }
            
            if (focusableCells.TryGet(column, row + 1, out var downCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)downCell.ChildInfo.Child, downKeys);
            }
        }

        return specBuilder.Build();
    }

    private void AddLinearFlow(IFocusable[] focusables, ConsoleKeyCollection changeKeys, FocusFlowSpecificationBuilder specBuilder)
    {
        for (int i = 0; i < focusables.Length - 1; i++)
        {
            specBuilder.AddFlow(focusables[i], focusables[i + 1], changeKeys);
        }

        if (FocusFlowLoop)
        {
            specBuilder.AddFlow(focusables[^1], focusables[0], changeKeys);
            return;
        }
        
        specBuilder.AddLoseFocus(focusables[^1], changeKeys);
    }

    private LineComposition CreateBorder(int width, int height, AbsoluteGridDefinition definition, LineCharSet charSet)
    {
        var linesBuilder = new LineCompositionBuilder(width, height)
        {
            Color = BorderColor,
            LineCharSet = charSet,
            OverlappingPriority = OverlappingPriority.Highest
        };

        int accumulatedLeft = 0;
        linesBuilder.Add(height, Orientation.Vertical, accumulatedLeft++, 0);
        
        for (int i = 0; i < definition.Columns.Count; i++)
        {
            accumulatedLeft += definition.Columns[i].Width;
            linesBuilder.Add(height, Orientation.Vertical, accumulatedLeft++, 0);
        }

        int accumulatedTop = 0;
        linesBuilder.Add(width, Orientation.Horizontal, 0, accumulatedTop++);
        
        for (int i = 0; i < definition.Rows.Count; i++)
        {
            accumulatedTop += definition.Rows[i].Width;
            linesBuilder.Add(width, Orientation.Horizontal, 0, accumulatedTop++);
        }

        return linesBuilder.Build(new UIElementBuildArgs(width, height));
    }

    private (GridCellsCollection, AbsoluteGridDefinition) ResolveGrid(int width, int height, bool hasBorders)
    {
        // Width and height without borders.
        int cleanWidth = hasBorders ? width - (1 + Definition.ColumnCount) : width;
        int cleanHeight = hasBorders ? height - (1 + Definition.RowCount) : height;
        
        var boxes = ResolveBoxes(cleanWidth, cleanHeight);

        var cleanPlacer = new ElementsFieldBuilder(cleanWidth, cleanHeight, false);

        var cells = new GridCell[Definition.ColumnCount, Definition.RowCount];

        int accumulatedCleanLeft = 0;

        for (int column = 0; column < Definition.ColumnCount; column++)
        {
            int leftDelta = 0;
            int accumulatedCleanTop = 0;
            
            for (int row = 0; row < Definition.RowCount; row++)
            {
                var canvasBuilder = boxes[column, row];

                var position = new Position(accumulatedCleanLeft, accumulatedCleanTop);

                cleanPlacer.Place(canvasBuilder, position, out var cellChild);
                
                accumulatedCleanTop += cellChild.Height;
                leftDelta = cellChild.Width;

                cells[column, row] = new GridCell(cellChild, column, row);
            }

            accumulatedCleanLeft += leftDelta;
        }

        var children = GetChildren(cells, hasBorders);

        var absoluteDefinition = GetAbsoluteDefinition(cells);

        return (children, absoluteDefinition);
    }

    private GridCellsCollection GetChildren(GridCell[,] cells, bool hasBorders)
    {
        return ResolvedPositionNotEmpty().ToCollection();
        
        IEnumerable<GridCell> ResolvedPositionNotEmpty()
        {
            for (int column = 0; column < cells.GetLength(0); column++)
            {
                for (int row = 0; row < cells.GetLength(1); row++)
                {
                    var cell = cells[column, row];

                    var wrapperCell = (Canvas)cell.ChildInfo.Child;

                    var singleChild = wrapperCell.Children.SingleOrDefault();
                    
                    if (singleChild is null)
                        continue;

                    int left = hasBorders
                        ? cell.ChildInfo.Left + singleChild.Left + column + 1
                        : cell.ChildInfo.Left + singleChild.Left;

                    int top = hasBorders
                        ? cell.ChildInfo.Top + singleChild.Top + row + 1
                        : cell.ChildInfo.Top + singleChild.Top;

                    var childInfo = new ChildInfo(singleChild.Child, left, top);

                    yield return new GridCell(childInfo, column, row);
                }
            }
        }
    }

    private AbsoluteGridDefinition GetAbsoluteDefinition(GridCell[,] cells)
    {
        var columns = Enumerable.Range(0, cells.GetLength(0))
            .Select(column => cells[column, 0].ChildInfo.Width)
            .Select(width => new AbsoluteGridColumn(width))
            .ToArray();
        
        var rows = Enumerable.Range(0, cells.GetLength(1))
            .Select(row => cells[0, row].ChildInfo.Height)
            .Select(height => new AbsoluteGridRow(height))
            .ToArray();

        return new AbsoluteGridDefinition(columns, rows);
    }

    private CanvasBuilder[,] ResolveBoxes(int width, int height)
    {
        var resolvedColumns = ResolveColumns(width);
        var resolvedRows = ResolveRows(height);

        var result = new CanvasBuilder[resolvedColumns.Length, resolvedRows.Length];

        for (int i = 0; i < resolvedColumns.Length; i++)
        {
            for (int j = 0; j < resolvedRows.Length; j++)
            {
                var size = ResolveSize(resolvedColumns[i], resolvedRows[j]);

                var canvasBuilder = new CanvasBuilder(size)
                {
                    FocusFlowLoop = false,
                    OverridesFocusFlow = false,
                    EnableOverlapping = false,
                };

                if (_queuedCellChildren[i, j] is {} queued)
                {
                    canvasBuilder.Add(queued.Builder, queued.Position);
                }

                result[i, j] = canvasBuilder;
            }
        }

        return result;
    }

    private Size ResolveSize((int? abs, double? rel) width, (int? abs, double? rel) height)
    {
        if (width.abs.HasValue)
        {
            return height.abs.HasValue
                ? new Size(width.abs.Value, height.abs.Value)
                : new Size(width.abs.Value, height.rel!.Value);
        }
        
        return height.abs.HasValue
            ? new Size(width.rel!.Value, height.abs.Value)
            : new Size(width.rel!.Value, height.rel!.Value);
    }

    private (int? abs, double? rel)[] ResolveColumns(int width)
    {
        var columns = Definition.ColumnDefinition;

        var result = new (int? abs, double? rel)[columns.Count];

        int absoluteSum = 0;
        double relationalSum= 0.0;
        double columnRelationSum = 0.0;
        
        for (int i = 0; i < columns.Count; i++)
        {
            var currentColumn = columns[i];
            
            if (currentColumn.IsAbsoluteWidth)
            {
                result[i] = (currentColumn.AbsoluteWidth, null);
                absoluteSum += currentColumn.AbsoluteWidth.Value;
                continue;
            }

            if (currentColumn.IsRelationalWidth)
            {
                result[i] = (null, currentColumn.RelationalWidth);
                relationalSum += currentColumn.RelationalWidth.Value;
                continue;
            }

            columnRelationSum += currentColumn.ColumnRelation!.Value;
        }

        double remainder = width - absoluteSum - relationalSum * width;
        
        if (remainder <= 0)
        {
            throw new GridDefinitionException("Absolute and relational column sizes overflow.");
        }

        for (int i = 0; i < columns.Count; i++)
        {
            if (!columns[i].IsColumnRelation)
                continue;

            double relational = remainder / width * (columns[i].ColumnRelation!.Value / columnRelationSum);
            result[i] = (null, relational);
        }

        return result;
    }

    private (int? abs, double? rel)[] ResolveRows(int height)
    {
        var rows = Definition.RowDefinition;

        var result = new (int? abs, double? rel)[rows.Count];

        int absoluteSum = 0;
        double relationalSum= 0.0;
        double rowRelationSum = 0.0;
        
        for (int i = 0; i < rows.Count; i++)
        {
            var currentRow = rows[i];
            
            if (currentRow.IsAbsoluteWidth)
            {
                result[i] = (currentRow.AbsoluteWidth, null);
                absoluteSum += currentRow.AbsoluteWidth.Value;
                continue;
            }

            if (currentRow.IsRelationalWidth)
            {
                result[i] = (null, currentRow.RelationalWidth);
                relationalSum += currentRow.RelationalWidth.Value;
                continue;
            }

            rowRelationSum += currentRow.RowRelation!.Value;
        }

        double remainder = height - absoluteSum - relationalSum * height;
        
        if (remainder <= 0)
        {
            throw new GridDefinitionException("Absolute and relational row sizes overflow.");
        }

        for (int i = 0; i < rows.Count; i++)
        {
            if (!rows[i].IsRowRelation)
                continue;

            double relational = remainder / height * (rows[i].RowRelation!.Value / rowRelationSum);
            result[i] = (null, relational);
        }

        return result;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public GridBuilder(int width, int height, GridDefinition definition)
        : this(new Size(width, height), definition)
    { }

    public GridBuilder(int width, double heightRelation, GridDefinition definition)
        : this(new Size(width, heightRelation), definition)
    { }

    public GridBuilder(double widthRelation, int height, GridDefinition definition)
        : this(new Size(widthRelation, height), definition)
    { }

    public GridBuilder(double widthRelation, double heightRelation, GridDefinition definition)
        : this(new Size(widthRelation, heightRelation), definition)
    { }
    
    public GridBuilder(Size size, GridDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));
        
        Size = size;
        Definition = definition;

        _queuedCellChildren = new QueuedPositionChild?[definition.ColumnCount, definition.RowCount];
    }
}