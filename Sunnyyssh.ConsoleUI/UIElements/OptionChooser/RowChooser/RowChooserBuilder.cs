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

    public Size Size { get; }
    
    public Orientation Orientation { get; }

    private readonly StackPanelBuilder _stackPanelBuilder;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;
    
    public OptionChooserKeySet? KeySet { get; init; }

    public bool CanChooseOnlyOne { get; init; } = true;

    public RowChooserBuilder Add(IUIElementBuilder<OptionElement> builder, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        _stackPanelBuilder.Add(builder, offset);

        return this;
    }
    
    public RowChooser Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var stackBuildArgs = new UIElementBuildArgs(width, height);
        var aggregatedStackPanel = _stackPanelBuilder.Build(stackBuildArgs);

        var orderedOptions = aggregatedStackPanel.Children
            // Here can't be any children except OptionElement.
            .Select(child => (OptionElement)child.Child)
            .ToCollection();

        var keySet = KeySet ?? (Orientation == Orientation.Vertical ? VerticalDefaultKeySet : HorizontalDefaultKeySet);

        var resultRowChooser = new RowChooser(width, height, aggregatedStackPanel,  orderedOptions,
            keySet, CanChooseOnlyOne, OverlappingPriority);

        return resultRowChooser;
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
        _stackPanelBuilder = new StackPanelBuilder(size, orientation);
    }
}