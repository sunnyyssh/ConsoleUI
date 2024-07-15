// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.Contracts;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sunnyyssh.ConsoleUI;

public sealed class GridBuilder : IUIElementBuilder<Grid>
{
    private static readonly ImmutableList<ConsoleKey> DefaultFocusChangeKeys 
        = new[] { ConsoleKey.Tab }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultFocusUpKeys 
        = new[] { ConsoleKey.UpArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultFocusDownKeys 
        = new[] { ConsoleKey.DownArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultFocusRightKeys 
        = new[] { ConsoleKey.RightArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultFocusLeftKeys 
        = new[] { ConsoleKey.LeftArrow }.ToImmutableList();
    
    private readonly QueuedPositionChild?[,] _queuedCellChildren;
    
    public Size Size { get; }

    public ImmutableList<ConsoleKey>? FocusChangeKeys { get; init; }

    public ImmutableList<ConsoleKey>? FocusUpKeys { get; init; }

    public ImmutableList<ConsoleKey>? FocusDownKeys { get; init; }

    public ImmutableList<ConsoleKey>? FocusRightKeys { get; init; }

    public ImmutableList<ConsoleKey>? FocusLeftKeys { get; init; }

    public bool FocusFlowLoop { get; init; } = false;

    public bool OverridesFocusFlow { get; init; } = true;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Color BorderColor { get; init; } = Color.Default;
    
    public LineCharSet? BorderLineCharSet { get; init; } = null;

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;
    
    public GridDefinition Definition { get; }

    public GridBuilder Add(IUIElementBuilder builder, int row, int column)
    {
        return Add(builder, row, column, Position.LeftTop);
    }

    public GridBuilder Add(IUIElementBuilder builder, int row, int column, Position position)
    {
        if (column < 0 || column >= Definition.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(column), column, null);
        
        if (row < 0 || row >= Definition.RowCount)
            throw new ArgumentOutOfRangeException(nameof(row), row, null);

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        if (_queuedCellChildren[row, column] is not null)
            throw new ArgumentException("Grid cell already has child.");

        _queuedCellChildren[row, column] = new QueuedPositionChild(builder, null, position);

        return this;
    }

    public GridBuilder Add(IUIElementBuilder builder, int row, int column, out BuiltUIElement builtUIElement)
        => Add(builder, row, column, Position.LeftTop, out builtUIElement);

    public GridBuilder Add(IUIElementBuilder builder, int row, int column, Position position, out BuiltUIElement builtUIElement)
    {
        if (column < 0 || column >= Definition.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(column), column, null);
        
        if (row < 0 || row >= Definition.RowCount)
            throw new ArgumentOutOfRangeException(nameof(row), row, null);

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        if (_queuedCellChildren[row, column] is not null)
            throw new ArgumentException("Grid cell already has child.");

        var initializer = new UIElementInitializer<UIElement>();
        builtUIElement = new BuiltUIElement(initializer);
        
        _queuedCellChildren[row, column] = new QueuedPositionChild(builder, initializer, position);

        return this;
    }

    public GridBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> builder, int row, int column, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(builder, row, column, Position.LeftTop, out builtUIElement);

    public GridBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> builder, int row, int column, Position position, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
    {
        if (column < 0 || column >= Definition.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(column), column, null);
        
        if (row < 0 || row >= Definition.RowCount)
            throw new ArgumentOutOfRangeException(nameof(row), row, null);

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        if (_queuedCellChildren[row, column] is not null)
            throw new ArgumentException("Grid cell already has child.");

        var initializer = new UIElementInitializer<TUIElement>();
        builtUIElement = new BuiltUIElement<TUIElement>(initializer);

        _queuedCellChildren[row, column] = new QueuedPositionChild(builder, initializer, position);

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

        var border = hasBorders ? CreateBorder(width, height, absoluteDefinition, borderCharSet!, BorderColor) : null;

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

    private FocusFlowSpecification CreateFocusSpecification(GridCellsCollection focusableCells, ImmutableList<ConsoleKey> changeKeys, 
        ImmutableList<ConsoleKey> upKeys, ImmutableList<ConsoleKey> downKeys, 
        ImmutableList<ConsoleKey> rightKeys, ImmutableList<ConsoleKey> leftKeys)
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
        
        foreach (var (childInfo, row, column) in focusableCells)
        {
            var currentFocusable = (IFocusable)childInfo.Child;
            
            if (focusableCells.TryGet(row, column - 1, out var leftCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)leftCell.ChildInfo.Child, leftKeys);
            }
            
            if (focusableCells.TryGet(row, column + 1, out var rightCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)rightCell.ChildInfo.Child, rightKeys);
            }
            
            if (focusableCells.TryGet(row - 1, column, out var upCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)upCell.ChildInfo.Child, upKeys);
            }
            
            if (focusableCells.TryGet(row + 1, column, out var downCell))
            {
                specBuilder.AddFlow(currentFocusable, (IFocusable)downCell.ChildInfo.Child, downKeys);
            }
        }

        return specBuilder.Build();
    }

    private void AddLinearFlow(IFocusable[] focusables, ImmutableList<ConsoleKey> changeKeys, FocusFlowSpecificationBuilder specBuilder)
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

    public static LineComposition CreateBorder(int width, int height, AbsoluteGridDefinition definition, 
        LineCharSet charSet, Color borderColor)
    {
        var linesBuilder = new LineCompositionBuilder(width, height)
        {
            Color = borderColor,
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
            accumulatedTop += definition.Rows[i].Height;
            linesBuilder.Add(width, Orientation.Horizontal, 0, accumulatedTop++);
        }

        return linesBuilder.Build(new UIElementBuildArgs(width, height));
    }

    [Pure]
    public static AbsoluteGridDefinition ResolveGrid(int width, int height, bool hasBorders, GridDefinition definition)
    {
        int cleanWidth = hasBorders ? width - (1 + definition.ColumnCount) : width;
        int cleanHeight = hasBorders ? height - (1 + definition.RowCount) : height;
        
        var resolvedColumns = ResolveColumns(cleanWidth, definition);
        var resolvedRows = ResolveRows(cleanHeight, definition);

        var boxes = new RectangleBuilder[resolvedRows.Length, resolvedColumns.Length];

        for (int row = 0; row < resolvedRows.Length; row++)
        {
            for (int column = 0; column < resolvedColumns.Length; column++)
            {
                var size = ResolveSize(resolvedColumns[column], resolvedRows[row]);

                boxes[row, column] = new RectangleBuilder(size, Color.Default);
            }
        }
        
        var cleanPlacer = new ElementsFieldBuilder(cleanWidth, cleanHeight, false);

        var cells = new GridCell[definition.RowCount, definition.ColumnCount];

        double accumulatedRelationalLeft = 0;

        for (int column = 0; column < definition.ColumnCount; column++)
        {
            double leftDelta = 0;
            double accumulatedRelationalTop = 0;
            
            for (int row = 0; row < definition.RowCount; row++)
            {
                var rectangleBuilder = boxes[row, column];

                var position = new Position(accumulatedRelationalLeft, accumulatedRelationalTop); 

                cleanPlacer.Place(rectangleBuilder, position, out var cellChild);
                
                accumulatedRelationalTop += rectangleBuilder.Size.IsHeightRelational
                    ? rectangleBuilder.Size.HeightRelation.Value
                    : (double)rectangleBuilder.Size.Height.Value / cleanHeight;
                
                leftDelta = rectangleBuilder.Size.IsWidthRelational
                    ? rectangleBuilder.Size.WidthRelation.Value
                    : (double)rectangleBuilder.Size.Width.Value / cleanWidth;
                
                cells[row, column] = new GridCell(cellChild, row, column);
            }

            accumulatedRelationalLeft += leftDelta;
        }

        var absoluteDefinition = GetAbsoluteDefinition(cells);

        return absoluteDefinition;
    }
    
    [Pure]
    private (GridCellsCollection, AbsoluteGridDefinition) ResolveGrid(int width, int height, bool hasBorders)
    {
        // Width and height without borders.
        int cleanWidth = hasBorders ? width - (1 + Definition.ColumnCount) : width;
        int cleanHeight = hasBorders ? height - (1 + Definition.RowCount) : height;
        
        var boxes = ResolveBoxes(cleanWidth, cleanHeight);

        var cleanPlacer = new ElementsFieldBuilder(cleanWidth, cleanHeight, false);

        var cells = new GridCell[Definition.RowCount, Definition.ColumnCount];

        double accumulatedRelationalLeft = 0;

        for (int column = 0; column < Definition.ColumnCount; column++)
        {
            double leftDelta = 0;
            double accumulatedRelationalTop = 0;
            
            for (int row = 0; row < Definition.RowCount; row++)
            {
                var (canvasBuilder, built, initializer) = boxes[row, column];

                var position = new Position(accumulatedRelationalLeft, accumulatedRelationalTop);

                cleanPlacer.Place(canvasBuilder, position, out var cellChild);
                
                // Here it's already built.
                if (built is not null && initializer is not null)
                {
                    if (!built.IsInitialized)
                        throw new InvalidOperationException();
                    
                    initializer.Initialize(built.Element);
                }

                accumulatedRelationalTop += canvasBuilder.Size.IsHeightRelational
                    ? canvasBuilder.Size.HeightRelation.Value
                    : (double)canvasBuilder.Size.Height.Value / cleanHeight;
                
                leftDelta = canvasBuilder.Size.IsWidthRelational
                    ? canvasBuilder.Size.WidthRelation.Value
                    : (double)canvasBuilder.Size.Width.Value / cleanWidth;

                cells[row, column] = new GridCell(cellChild, row, column);
            }

            accumulatedRelationalLeft += leftDelta;
        }

        var children = GetChildren(cells, hasBorders);

        var absoluteDefinition = GetAbsoluteDefinition(cells);

        return (children, absoluteDefinition);
    }

    [Pure]
    private GridCellsCollection GetChildren(GridCell[,] cells, bool hasBorders)
    {
        return ResolvedPositionNotEmpty().ToCollection();
        
        IEnumerable<GridCell> ResolvedPositionNotEmpty()
        {
            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int column = 0; column < cells.GetLength(1); column++)
                {
                    var cell = cells[row, column];

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

                    yield return new GridCell(childInfo, row, column);
                }
            }
        }
    }

    [Pure]
    private static AbsoluteGridDefinition GetAbsoluteDefinition(GridCell[,] cells)
    {
        var columns = Enumerable.Range(0, cells.GetLength(1))
            .Select(column => cells[0, column].ChildInfo.Width)
            .Select(width => new AbsoluteGridColumn(width))
            .ToImmutableList();
        
        var rows = Enumerable.Range(0, cells.GetLength(0))
            .Select(row => cells[row, 0].ChildInfo.Height)
            .Select(height => new AbsoluteGridRow(height))
            .ToImmutableList();

        return new AbsoluteGridDefinition(columns, rows);
    }

    [Pure]
    private (CanvasBuilder canvasBuilder, BuiltUIElement? built, IUIElementInitializer? initializer)[,] 
        ResolveBoxes(int width, int height)
    {
        var resolvedColumns = ResolveColumns(width, Definition);
        var resolvedRows = ResolveRows(height, Definition);

        var result = new (CanvasBuilder, BuiltUIElement?, IUIElementInitializer?)
            [resolvedRows.Length, resolvedColumns.Length];

        for (int column = 0; column < resolvedColumns.Length; column++)
        {
            for (int row = 0; row < resolvedRows.Length; row++)
            {
                var size = ResolveSize(resolvedColumns[column], resolvedRows[row]);

                var canvasBuilder = new CanvasBuilder(size)
                {
                    FocusFlowLoop = false,
                    OverridesFocusFlow = false,
                    EnableOverlapping = false,
                };

                (CanvasBuilder, BuiltUIElement?, IUIElementInitializer?) toAdd = (canvasBuilder, null, null);

                if (_queuedCellChildren[row, column] is {} queued)
                {
                    if (queued.Initializer is null)
                    {
                        canvasBuilder.Add(queued.Builder, queued.Position);
                    }
                    else
                    {
                        canvasBuilder.Add(queued.Builder, queued.Position, out var builtUIElement);
                        toAdd = (canvasBuilder, builtUIElement, queued.Initializer);
                    }
                }

                result[row, column] = toAdd;
            }
        }

        return result;
    }

    [Pure]
    private static Size ResolveSize((int? abs, double? rel) width, (int? abs, double? rel) height)
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

    [Pure]
    private static (int? abs, double? rel)[] ResolveColumns(int width, GridDefinition definition)
    {
        var columns = definition.ColumnDefinition;

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

    [Pure]
    private static (int? abs, double? rel)[] ResolveRows(int height, GridDefinition definition)
    {
        var rows = definition.RowDefinition;

        var result = new (int? abs, double? rel)[rows.Count];

        int absoluteSum = 0;
        double relationalSum= 0.0;
        double rowRelationSum = 0.0;
        
        for (int i = 0; i < rows.Count; i++)
        {
            var currentRow = rows[i];
            
            if (currentRow.IsAbsoluteHeight)
            {
                result[i] = (currentRow.AbsoluteHeight, null);
                absoluteSum += currentRow.AbsoluteHeight.Value;
                continue;
            }

            if (currentRow.IsRelationalHeight)
            {
                result[i] = (null, currentRow.RelationalHeight);
                relationalSum += currentRow.RelationalHeight.Value;
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

        _queuedCellChildren = new QueuedPositionChild?[definition.RowCount, definition.ColumnCount];
    }
}