namespace Sunnyyssh.ConsoleUI;

public sealed class TextBoxBuilder : IUIElementBuilder<TextBox>
{
    #region Colors.
    
    private readonly Color? _focusedBackground;

    private readonly Color? _focusedForeground;

    private readonly Color? _focusedBorderColor;

    private readonly Color _notFocusedBackground = Color.Default;

    private readonly Color _notFocusedForeground = Color.Default;

    private readonly Color _notFocusedBorderColor = Color.Default;

    public Color Background { get; private set; } = Color.Default;
    
    public Color Foreground { get; private set; } = Color.Default;
    
    public Color BorderColor { get; private set; } = Color.Default;

    public Color NotFocusedBackground
    {
        get => _notFocusedBackground;
        init => _notFocusedBackground = Background = value;
    }

    public Color NotFocusedForeground
    {
        get => _notFocusedForeground;
        init => _notFocusedForeground = Foreground = value;
    }

    public Color NotFocusedBorderColor
    {
        get => _notFocusedBorderColor;
        init => _notFocusedBorderColor = BorderColor = value;
    }

    public Color FocusedBackground
    {
        get => _focusedBackground ?? NotFocusedBackground;
        init => _focusedBackground = value;
    }

    public Color FocusedForeground
    {
        get => _focusedForeground ?? NotFocusedForeground;
        init => _focusedForeground = value;
    }

    public Color FocusedBorderColor
    {
        get => _focusedBorderColor ?? NotFocusedBorderColor;
        init => _focusedBorderColor = value;
    }
    
    #endregion
    
    public Size Size { get; }

    public string? StartingText { get; init; } = null;

    public IObservable<string>? BoundObservable { get; init; } = null;

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public bool ShowPressedChars { get; init; } = true;

    public bool WordWrap { get; init; } = true;

    public bool UserEditable { get; init; } = true;
    
    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Left;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Top;

    public event CharEnteredEventHandler? CharEntered;

    public event TextEnteredEventHandler? TextEntered;
    
    public TextBox Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;
        
        var resultTextBox = new TextBox(width, height, OverlappingPriority)
        {
            BorderKind = BorderKind,
            WordWrap = WordWrap,
            TextHorizontalAligning = TextHorizontalAligning,
            TextVerticalAligning = TextVerticalAligning,
            FocusedBackground = FocusedBackground,
            FocusedBorderColor = FocusedBorderColor,
            FocusedForeground = FocusedForeground,
            NotFocusedBackground = NotFocusedBackground,
            NotFocusedBorderColor = NotFocusedBorderColor,
            NotFocusedForeground = NotFocusedForeground,
            ShowPressedChars = ShowPressedChars,
            UserEditable = UserEditable,
            Text = StartingText,
        };
        
        if (CharEntered is not null)
        {
            resultTextBox.CharEntered += CharEntered;
        }
        
        if (TextEntered is not null)
        {
            resultTextBox.TextEntered += TextEntered;
        }

        if (BoundObservable is not null)
        {
            resultTextBox.Bind(BoundObservable);
        }

        return resultTextBox;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public TextBoxBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        
        Size = size;
    }
}