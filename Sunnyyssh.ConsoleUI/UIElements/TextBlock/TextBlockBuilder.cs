namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlockBuilder : IUIElementBuilder<TextBlock>
{
    private string? StartingText { get; init; }

    public IObservable<string>? BoundObservable { get; init; }

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

        if (BoundObservable is not null)
        {
            resultTextBlock.Bind(BoundObservable);
        }

        return resultTextBlock;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public TextBlockBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
    }
}