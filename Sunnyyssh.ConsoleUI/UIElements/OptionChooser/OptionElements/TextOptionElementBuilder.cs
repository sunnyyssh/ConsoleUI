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

    public TextOptionElementBuilder(Size size, string text, TextOptionColorSet colorSet)
    {
        Size = size;
        Text = text;
        ColorSet = colorSet;
    }
}