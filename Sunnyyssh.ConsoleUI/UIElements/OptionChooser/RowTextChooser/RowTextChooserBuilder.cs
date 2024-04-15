using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace Sunnyyssh.ConsoleUI;

public sealed class RowTextChooserBuilder : IUIElementBuilder<RowTextChooser>
{
    private static readonly OptionChooserKeySet HorizontalDefaultKeySet =
        new OptionChooserKeySet(
            new[] { ConsoleKey.RightArrow, ConsoleKey.D },
            new[] { ConsoleKey.LeftArrow, ConsoleKey.A },
            new[] { ConsoleKey.Enter },
            new[] { ConsoleKey.Enter });

    private static readonly OptionChooserKeySet VerticalDefaultKeySet =
        new OptionChooserKeySet(
            new[] { ConsoleKey.DownArrow, ConsoleKey.S },
            new[] { ConsoleKey.UpArrow, ConsoleKey.W },
            new[] { ConsoleKey.Enter },
            new[] { ConsoleKey.Enter });

    private readonly List<string> _options;

    public OptionChooserKeySet? KeySet { get; init; }

    public bool CanChooseOnlyOne { get; init; } = true;

    public Size Size { get; }

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;
    
    public LineCharSet? BorderLineCharSet { get; init; }

    public Color BorderColor { get; init; } = Color.Default;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public string[] Options => _options.ToArray();

    public Orientation Orientation { get; }

    public TextOptionColorSet ColorSet { get; init; } = 
        new TextOptionColorSet(Color.Default, Color.Default);

    public RowTextChooserBuilder Add(string option)
    {
        ArgumentNullException.ThrowIfNull(option, nameof(option));

        _options.Add(option);

        return this;
    }

    public RowTextChooser Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        int width = args.Width;
        int height = args.Height;
        
        var options = _options.ToArray();

        Grid initGrid;

        if (Orientation == Orientation.Vertical)
        {
            var initOptions = InitializeOptionsVertical(options);
            initGrid = InitializeVerticalGrid(width, height, initOptions);
        }
        else
        {
            var initOptions = InitializeOptionsHorizontal(options);
            initGrid = InitializeHorizontalGrid(width, height, initOptions);
        }

        var resultElements = initGrid.Children
            .Where(ch => ch.Child is TextOptionElement)
            // Only TextOptionElement children can be here.
            .Select(ch => (TextOptionElement)ch.Child)
            .ToImmutableList();

        var keySet = KeySet ?? (Orientation == Orientation.Vertical ? VerticalDefaultKeySet : HorizontalDefaultKeySet);

        var resultChooser = new RowTextChooser(width, height, 
            initGrid, resultElements, keySet, 
            CanChooseOnlyOne, OverlappingPriority);
        
        return resultChooser;
    }
    private Grid InitializeVerticalGrid(int width, int height, IUIElementBuilder<TextOptionElement>[] initOptions)
    {
        var rows = initOptions
            .Select(builder => builder.Size.IsHeightRelational
                ? GridRow.FromRowRelation(builder.Size.HeightRelation.Value)
                : GridRow.FromHeight(builder.Size.Height.Value));

        var columns = Enumerable.Repeat(GridColumn.FromColumnRelation(1), 1);

        var gridDefinition = new GridDefinition(
            GridRowDefinition.From(rows),
            GridColumnDefinition.From(columns));

        var gridBuilder = new GridBuilder(width, height, gridDefinition)
        {
            BorderLineCharSet = BorderLineCharSet,
            BorderKind = BorderKind,
            BorderColor = BorderColor,
        };
        
        for (int row = 0; row < initOptions.Length; row++)
        {
            gridBuilder.Add(initOptions[row].UnsafeWithSize(Size.FullSize), row, 0);
        }

        return gridBuilder.Build(new UIElementBuildArgs(width, height));
    }

    private Grid InitializeHorizontalGrid(int width, int height, IUIElementBuilder<TextOptionElement>[] initOptions)
    {
        var rows = Enumerable.Repeat(GridRow.FromRowRelation(1), 1);

        var columns = initOptions
            .Select(builder => builder.Size.IsWidthRelational
                ? GridColumn.FromColumnRelation(builder.Size.WidthRelation.Value)
                : GridColumn.FromWidth(builder.Size.Width.Value));

        var gridDefinition = new GridDefinition(
            GridRowDefinition.From(rows),
            GridColumnDefinition.From(columns));

        var gridBuilder = new GridBuilder(width, height, gridDefinition)
        {
            BorderLineCharSet = BorderLineCharSet,
            BorderKind = BorderKind,
            BorderColor = BorderColor,
        };
        
        for (int column = 0; column < initOptions.Length; column++)
        {
            gridBuilder.Add(initOptions[column].UnsafeWithSize(Size.FullSize), 0, column);
        }

        return gridBuilder.Build(new UIElementBuildArgs(width, height));
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    private IUIElementBuilder<TextOptionElement>[] InitializeOptionsVertical(string[] options)
    {
        int count = options.Length;

        double heightRelational = 1.0 / count;
        const double wideWidth = 1.0;

        var result = new IUIElementBuilder<TextOptionElement>[count];

        for (int i = 0; i < count; i++)
        {
            var textOptionBuilder = new TextOptionElementBuilder(new Size(wideWidth, heightRelational), options[i], ColorSet);

            result[i] = textOptionBuilder;
        }

        return result;
    }

    private IUIElementBuilder<TextOptionElement>[] InitializeOptionsHorizontal(string[] options)
    {
        int count = options.Length;

        double widthRelational = 1.0 / count;
        const double wideHeight = 1.0;

        var result = new IUIElementBuilder<TextOptionElement>[count];

        for (int i = 0; i < count; i++)
        {
            var textOptionBuilder = new TextOptionElementBuilder(new Size(widthRelational, wideHeight), options[i], ColorSet);

            result[i] = textOptionBuilder;
        }

        return result;
    }

    public RowTextChooserBuilder(int width, int height, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
        : this(new Size(width, height), orientation, initOptions)
    { }
    
    public RowTextChooserBuilder(int width, double heightRelation, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
        : this(new Size(width, heightRelation), orientation, initOptions)
    { }
    
    public RowTextChooserBuilder(double widthRelation, int height, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
        : this(new Size(widthRelation, height), orientation, initOptions)
    { }
    
    public RowTextChooserBuilder(double widthRelation, double heightRelation, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
        : this(new Size(widthRelation, heightRelation), orientation, initOptions)
    { }
    
    public RowTextChooserBuilder(Size size, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
        Orientation = orientation;
        _options = initOptions?.ToList() ?? new List<string>();
    }
}