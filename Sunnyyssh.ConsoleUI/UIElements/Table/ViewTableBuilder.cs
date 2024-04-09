using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public class ViewTableBuilder : IUIElementBuilder<ViewTable>
{
    private static readonly ImmutableList<ConsoleKey> DefaultMoveUpKeys 
        = new[] { ConsoleKey.UpArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultMoveDownKeys 
        = new[] { ConsoleKey.DownArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultMoveRightKeys 
        = new[] { ConsoleKey.RightArrow }.ToImmutableList();
    
    private static readonly ImmutableList<ConsoleKey> DefaultMoveLeftKeys 
        = new[] { ConsoleKey.LeftArrow }.ToImmutableList();
    
    private readonly ImmutableList<ImmutableList<string?>> _initData;
    
    private readonly GridDefinition _gridDefinition;
    
    private readonly Color? _cellFocusedBackground = Color.DarkGray;
    
    private readonly Color? _cellFocusedForeground;
    
    private readonly Color? _borderColorFocused;

    public ImmutableList<string> Headers { get; }
    
    public Size Size { get; }
    
    #region init properties.
     
    public bool EnterOnCellChange { get; init; }

    public bool LoseFocusOnEnter { get; init; } = false;
    
    public LineKind BorderLineKind { get; init; } = LineKind.Single;

    public LineCharSet? BorderCharSet { get; init; } = null;
    
    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;
     
    public Color CellNotFocusedBackground { get; init; } = Color.Default;

    public Color HeaderBackground { get; init; } = Color.Default;

    public Color CellNotFocusedForeground { get; init; } = Color.Default;

    public Color HeaderForeground { get; init; } = Color.Default;

    public Color CellFocusedBackground
    {
        get => _cellFocusedBackground ?? CellNotFocusedBackground;
        init => _cellFocusedBackground = value;
    }

    public Color CellFocusedForeground
    {
        get => _cellFocusedForeground ?? CellNotFocusedForeground;
        init => _cellFocusedForeground = value;
    }

    public Color BorderColorNotFocused { get; init; } = Color.Default;

    public Color BorderColorFocused
    {
        get => _borderColorFocused ?? BorderColorNotFocused;
        init => _borderColorFocused = value;
    }

    public ImmutableList<ConsoleKey>? MoveRightKeys { get; init; } = DefaultMoveRightKeys;

    public ImmutableList<ConsoleKey>? MoveLeftKeys { get; init; } = DefaultMoveLeftKeys;

    public ImmutableList<ConsoleKey>? MoveUpKeys { get; init; } = DefaultMoveUpKeys;

    public ImmutableList<ConsoleKey>? MoveDownKeys { get; init; } = DefaultMoveDownKeys;

    public bool CellsWordWrap { get; init; } = true;

    public bool UserEditable { get; init; } = true;
     
    #endregion

    public ViewTable Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        int width = args.Width;
        int height = args.Height;

        var borderCharSet = BorderCharSet ?? LineCharSets.Of(BorderLineKind);

        var absoluteGridDefinition = GridBuilder.ResolveGrid(width, height, true, _gridDefinition);

        var resultViewTable = new ViewTable(width, height, Headers, borderCharSet, 
            _initData, absoluteGridDefinition, OverlappingPriority)
        {
            CellNotFocusedBackground = CellNotFocusedBackground,
            HeaderBackground = HeaderBackground,
            CellNotFocusedForeground = CellNotFocusedForeground,
            HeaderForeground = HeaderForeground,
            CellFocusedBackground = CellFocusedBackground,
            CellFocusedForeground = CellFocusedForeground,
            BorderColorNotFocused = BorderColorNotFocused,
            BorderColorFocused = BorderColorFocused,
            MoveRightKeys = MoveRightKeys,
            MoveLeftKeys = MoveLeftKeys,
            MoveUpKeys = MoveUpKeys,
            MoveDownKeys = MoveDownKeys,
            CellsWordWrap = CellsWordWrap,
            UserEditable = UserEditable,
            LoseFocusOnEnter = LoseFocusOnEnter,
            EnterOnCellChange = EnterOnCellChange,
        };

        return resultViewTable;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    private static ImmutableList<ImmutableList<string?>> CreateEmptyInitValues(int columnsCount, int dataRowsCount)
    {
        return Enumerable.Repeat(
                Enumerable.Repeat((string?)null, columnsCount)
                    .ToImmutableList(),
                dataRowsCount)
            .ToImmutableList();
    }

    #region Constructors
    
    public ViewTableBuilder(int width, int height, ImmutableList<string> headers, GridDefinition gridDefinition, int dataRowsCount)
        : this(new Size(width, height), headers, gridDefinition, dataRowsCount)
    { }

    public ViewTableBuilder(int width, double heightRelational, ImmutableList<string> headers, GridDefinition gridDefinition, int dataRowsCount)
        : this(new Size(width, heightRelational), headers, gridDefinition, dataRowsCount)
    { }

    public ViewTableBuilder(double widthRelational, int height, ImmutableList<string> headers, GridDefinition gridDefinition, int dataRowsCount)
        : this(new Size(widthRelational, height), headers, gridDefinition, dataRowsCount)
    { }

    public ViewTableBuilder(double widthRelational, double heightRelational, ImmutableList<string> headers, GridDefinition gridDefinition, int dataRowsCount)
        : this(new Size(widthRelational, heightRelational), headers, gridDefinition, dataRowsCount)
    { }
    
    public ViewTableBuilder(Size size, ImmutableList<string> headers, GridDefinition gridDefinition, int dataRowsCount)
        : this(size, headers, gridDefinition, CreateEmptyInitValues(headers.Count, dataRowsCount))
    { }

    public ViewTableBuilder(int width, int height, ImmutableList<string> headers, GridDefinition? gridDefinition,
        ImmutableList<ImmutableList<string?>> initData)
        : this(new Size(width, height), headers, gridDefinition, initData)
    { }

    public ViewTableBuilder(int width, double heightRelational, ImmutableList<string> headers, GridDefinition? gridDefinition,
        ImmutableList<ImmutableList<string?>> initData)
        : this(new Size(width, heightRelational), headers, gridDefinition, initData)
    { }

    public ViewTableBuilder(double widthRelational, int height, ImmutableList<string> headers, GridDefinition? gridDefinition,
        ImmutableList<ImmutableList<string?>> initData)
        : this(new Size(widthRelational, height), headers, gridDefinition, initData)
    { }

    public ViewTableBuilder(double widthRelational, double heightRelational, ImmutableList<string> headers, GridDefinition? gridDefinition,
        ImmutableList<ImmutableList<string?>> initData)
        : this(new Size(widthRelational, heightRelational), headers, gridDefinition, initData)
    { }
    
    public ViewTableBuilder(Size size, ImmutableList<string> headers, GridDefinition? gridDefinition,
         ImmutableList<ImmutableList<string?>> initData)
    {
        ArgumentNullException.ThrowIfNull(headers, nameof(headers));
        ArgumentNullException.ThrowIfNull(gridDefinition, nameof(gridDefinition));
        ArgumentNullException.ThrowIfNull(initData, nameof(initData));

        _initData = initData;
        _gridDefinition = gridDefinition;
        Size = size;
        Headers = headers;
    }

    #endregion
}