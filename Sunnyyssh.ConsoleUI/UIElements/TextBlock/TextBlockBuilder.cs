namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlockBuilder : IUIElementBuilder<TextBlock>
{
    public string? StartingText { get; init; }

    public Color Background { get; init; } = Color.Default;

    public Color Foreground { get; init; } = Color.Default;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Center;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Center;

    public bool WordWrap { get; init; } = true;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Size Size { get; }

    public TextBlock Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
   
        int width = args.Width;
        int height = args.Height;

        var resultTextBlock = new TextBlock(width, height, OverlappingPriority)
        {
            Background = Background,
            Foreground = Foreground,
            TextVerticalAligning = TextVerticalAligning,
            TextHorizontalAligning = TextHorizontalAligning,
            WordWrap = WordWrap,
            Text = StartingText,
        };

        return resultTextBlock;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public TextBlockBuilder(int width, int height)
        : this(new Size(width, height))
    { }

    public TextBlockBuilder(int width, double heightRelation)
        : this(new Size(width, heightRelation))
    { }

    public TextBlockBuilder(double widthRelation, int height)
        : this(new Size(widthRelation, height))
    { }

    public TextBlockBuilder(double widthRelation, double heightRelation)
        : this(new Size(widthRelation, heightRelation))
    { }
    
    public TextBlockBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
    }
}