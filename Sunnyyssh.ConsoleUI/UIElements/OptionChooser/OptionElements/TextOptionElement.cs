using System.Collections.Concurrent;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextOptionElement : StateOptionElement
{
    private readonly ConcurrentDictionary<(bool isChosen, bool isFocused), DrawState> _textPresentations = new();

    public TextOptionColorSet ColorSet { get; }

    public bool WordWrap { get; init; } = true;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Center;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Center;
    
    public string Text { get; }

    protected override DrawState RequestState(bool isChosen, bool isFocused)
    {
        if (!_textPresentations.TryGetValue((isChosen, isFocused), out var drawState))
        {
            drawState = CreateState(isChosen, isFocused);
        }

        _textPresentations.TryAdd((isChosen, isFocused), drawState);

        return drawState;
    }

    private DrawState CreateState(bool isChosen, bool isFocused)
    {
        var (background, foreground) = (isChosen, isFocused) switch
        {
            (false, false) => 
                (ColorSet.BaseBackground, ColorSet.BaseForeground),
            (false, true) => 
                (ColorSet.FocusedBackground, ColorSet.FocusedForeground),
            (true, false) => 
                (ColorSet.ChosenBackground, ColorSet.ChosenForeground),
            (true, true) => 
                (ColorSet.ChosenFocusedBackground, ColorSet.ChosenFocusedForeground),
        };

        var builder = new DrawStateBuilder(Width, Height);

        builder.Fill(background);
        
        TextHelper.PlaceText(0, 0, Width, Height, 
            WordWrap, Text, background, foreground, 
            TextVerticalAligning, TextHorizontalAligning, builder);

        var state = builder.ToDrawState();

        return state;
    }

    public TextOptionElement(int width, int height, TextOptionColorSet colorSet, string? text) : base(width, height)
    {
        ColorSet = colorSet;
        Text = text ?? string.Empty;
    }
}