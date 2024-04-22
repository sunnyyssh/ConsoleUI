// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBoxBuilder : IUIElementBuilder<TextBox>
{
    #region Colors.
    
    private readonly Color? _focusedBackground;

    private readonly Color? _focusedForeground;

    private readonly Color? _focusedBorderColor;

    public Color NotFocusedBackground { get; init; } = Color.Default;

    public Color NotFocusedForeground { get; init; } = Color.Default;

    public Color NotFocusedBorderColor { get; init; } = Color.Default;

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

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public bool ShowPressedChars { get; init; } = true;

    public bool WordWrap { get; init; } = true;

    public bool UserEditable { get; init; } = true;
    
    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Left;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Top;
    
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

        return resultTextBox;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public TextBoxBuilder(int width, int height)
        : this(new Size(width, height))
    { }
    
    public TextBoxBuilder(int width, double heightRelation)
        : this(new Size(width, heightRelation))
    { }
    
    public TextBoxBuilder(double widthRelation, int height)
        : this(new Size(widthRelation, height))
    { }
    
    public TextBoxBuilder(double widthRelation, double heightRelation)
        : this(new Size(widthRelation, heightRelation))
    { }
    
    public TextBoxBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        
        Size = size;
    }
}