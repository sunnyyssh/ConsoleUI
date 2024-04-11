using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class RowChooserBuilder : IUIElementBuilder<RowChooser>
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

    private readonly List<QueuedChild> _queuedChildren = new();
    
    public Size Size { get; }
    
    public Orientation Orientation { get; }

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;
    
    public LineCharSet? BorderLineCharSet { get; init; }

    public Color BorderColor { get; init; } = Color.Default;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;
    
    public OptionChooserKeySet? KeySet { get; init; }

    public bool CanChooseOnlyOne { get; init; } = true;

    public RowChooserBuilder Add(IUIElementBuilder<OptionElement> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        _queuedChildren.Add(new QueuedChild(builder, null));

        return this;
    }

    public RowChooserBuilder Add<TOptionElement>(IUIElementBuilder<TOptionElement> builder, 
        out BuiltUIElement<TOptionElement> builtUIElement)
        where TOptionElement : OptionElement
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var initializer = new UIElementInitializer<TOptionElement>();
        builtUIElement = new BuiltUIElement<TOptionElement>(initializer);
        
        _queuedChildren.Add(new QueuedChild(builder, initializer));

        return this;
    }
    
    public RowChooser Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var aggregatedGrid = Orientation == Orientation.Horizontal
            ? InitializeHorizontalGrid(width, height)
            : InitializeVerticalGrid(width, height);

        var orderedOptions = aggregatedGrid.Children
            .Where(child => child.Child is OptionElement)
            // Here can't be any children except OptionElement.
            .Select(child => (OptionElement)child.Child)
            .ToImmutableList();

        var keySet = KeySet ?? (Orientation == Orientation.Vertical ? VerticalDefaultKeySet : HorizontalDefaultKeySet);

        var resultRowChooser = new RowChooser(width, height, aggregatedGrid,  orderedOptions,
            keySet, CanChooseOnlyOne, OverlappingPriority);

        return resultRowChooser;
    }

    private Grid InitializeVerticalGrid(int width, int height)
    {
        var rows = _queuedChildren
            .Select(builder => builder.Builder.Size.IsHeightRelational
                ? GridRow.FromRowRelation(builder.Builder.Size.HeightRelation.Value)
                : GridRow.FromHeight(builder.Builder.Size.Height.Value));

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

        Dictionary<IUIElementInitializer, BuiltUIElement> builtInitializers = new();
        
        for (int row = 0; row < _queuedChildren.Count; row++)
        {
            var currentQueued = _queuedChildren[row];
            var fullSizeBuilder = currentQueued.Builder.UnsafeWithSize(Size.FullSize);
            
            if (currentQueued.Initializer is null)
            {
                gridBuilder.Add(fullSizeBuilder, row, 0);
                continue;
            }

            gridBuilder.Add(fullSizeBuilder, row, 0, out var builtUIElement);
            builtInitializers.Add(currentQueued.Initializer, builtUIElement);
        }

        var result = gridBuilder.Build(new UIElementBuildArgs(width, height));

        foreach (var (initializer, builtUIElement) in builtInitializers)
        {
            if (!builtUIElement.IsInitialized)
                throw new InvalidOperationException();
            
            initializer.Initialize(builtUIElement.Element);
        }

        return result;
    }

    private Grid InitializeHorizontalGrid(int width, int height)
    {
        var rows = Enumerable.Repeat(GridRow.FromRowRelation(1), 1);

        var columns = _queuedChildren
            .Select(builder => builder.Builder.Size.IsWidthRelational
                ? GridColumn.FromColumnRelation(builder.Builder.Size.WidthRelation.Value)
                : GridColumn.FromWidth(builder.Builder.Size.Width.Value));

        var gridDefinition = new GridDefinition(
            GridRowDefinition.From(rows),
            GridColumnDefinition.From(columns));

        var gridBuilder = new GridBuilder(width, height, gridDefinition)
        {
            BorderLineCharSet = BorderLineCharSet,
            BorderKind = BorderKind,
            BorderColor = BorderColor,
        };
        
        
        Dictionary<IUIElementInitializer, BuiltUIElement> builtInitializers = new();
        
        for (int column = 0; column < _queuedChildren.Count; column++)
        {
            var currentQueued = _queuedChildren[column];
            var fullSizeBuilder = currentQueued.Builder.UnsafeWithSize(Size.FullSize);
            
            if (currentQueued.Initializer is null)
            {
                gridBuilder.Add(fullSizeBuilder, 0, column);
                continue;
            }

            gridBuilder.Add(fullSizeBuilder, 0, column, out var builtUIElement);
            builtInitializers.Add(currentQueued.Initializer, builtUIElement);
        }

        var result = gridBuilder.Build(new UIElementBuildArgs(width, height));

        foreach (var (initializer, builtUIElement) in builtInitializers)
        {
            if (!builtUIElement.IsInitialized)
                throw new InvalidOperationException();
            
            initializer.Initialize(builtUIElement.Element);
        }

        return result;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public RowChooserBuilder(int width, int height, Orientation orientation)
        : this(new Size(width, height), orientation)
    { }

    public RowChooserBuilder(int width, double heightRelation, Orientation orientation)
        : this(new Size(width, heightRelation), orientation)
    { }

    public RowChooserBuilder(double widthRelation, int height, Orientation orientation)
        : this(new Size(widthRelation, height), orientation)
    { }

    public RowChooserBuilder(double widthRelation, double heightRelation, Orientation orientation)
        : this(new Size(widthRelation, heightRelation), orientation)
    { }
    
    public RowChooserBuilder(Size size, Orientation orientation)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        
        Size = size;
        Orientation = orientation;
    }
}