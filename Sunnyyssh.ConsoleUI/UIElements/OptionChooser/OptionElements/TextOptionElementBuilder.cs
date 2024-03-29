namespace Sunnyyssh.ConsoleUI;

public sealed class TextOptionElementBuilder : IUIElementBuilder<TextOptionElement>
{
    public Size Size { get; }

    public TextOptionColorSet ColorSet { get; }

    public bool WordWrap { get; init; } = true;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Center;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Center;
    
    public string Text { get; }
    
    public TextOptionElement Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var textOption = new TextOptionElement(width, height, ColorSet, Text)
        {
            TextHorizontalAligning = TextHorizontalAligning,
            TextVerticalAligning = TextVerticalAligning,
            WordWrap = WordWrap,
        };

        return textOption;  
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public TextOptionElementBuilder(int width, int height, string text, TextOptionColorSet colorSet)
        : this(new Size(width, height), text, colorSet)
    { }

    public TextOptionElementBuilder(int width, double heightRelation, string text, TextOptionColorSet colorSet)
        : this(new Size(width, heightRelation), text, colorSet)
    { }

    public TextOptionElementBuilder(double widthRelation, int height, string text, TextOptionColorSet colorSet)
        : this(new Size(widthRelation, height), text, colorSet)
    { }

    public TextOptionElementBuilder(double widthRelation, double heightRelation, string text, TextOptionColorSet colorSet)
        : this(new Size(widthRelation, heightRelation), text, colorSet)
    { }
    
    public TextOptionElementBuilder(Size size, string text, TextOptionColorSet colorSet)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        ArgumentNullException.ThrowIfNull(colorSet, nameof(colorSet));

        Size = size;
        Text = text;
        ColorSet = colorSet;
    }
}