namespace Sunnyyssh.ConsoleUI;

public sealed class TextOptionColorSet
{
    private readonly Color? _focusedBackground;
    private readonly Color? _focusedForeground;
    private readonly Color? _chosenBackground = Color.Gray;
    private readonly Color? _chosenForeground;
    private readonly Color? _chosenFocusedBackground;
    private readonly Color? _chosenFocusedForeground;

    public Color BaseBackground { get; }

    public Color BaseForeground { get; }

    public Color FocusedBackground
    {
        get => _focusedBackground ?? BaseBackground;
        init => _focusedBackground = value;
    }

    public Color FocusedForeground
    {
        get => _focusedForeground ?? BaseForeground;
        init => _focusedForeground = value;
    }

    public Color ChosenBackground
    {
        get => _chosenBackground ?? FocusedBackground;
        init => _chosenBackground = value;
    }

    public Color ChosenForeground
    {
        get => _chosenForeground ?? FocusedForeground;
        init => _chosenForeground = value;
    }

    public Color ChosenFocusedBackground
    {
        get => _chosenFocusedBackground ?? ChosenBackground;
        init => _chosenFocusedBackground = value;
    }

    public Color ChosenFocusedForeground
    {
        get => _chosenFocusedForeground ?? ChosenForeground;
        init => _chosenFocusedForeground = value;
    }

    public TextOptionColorSet(Color baseBackground, Color baseForeground)
    {
        BaseBackground = baseBackground;
        BaseForeground = baseForeground;
    }
}