using System.Collections.Immutable;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sunnyyssh.ConsoleUI;

public sealed class ButtonBuilder : IUIElementBuilder<Button>
{
    #region Properties
    
    private ImmutableList<ConsoleKey> _ignoredKeys = ImmutableList<ConsoleKey>.Empty;

    public OverlappingPriority OverlappingPriority { get; set; } = OverlappingPriority.Medium;
    
    public Color NotFocusedBackground { get; set; } = Color.Default;
    
    public Color NotFocusedForeground { get; set; } = Color.Default;

    public Color NotFocusedBorderColor { get; set; } = Color.Default;

    public Color? FocusedBackground { get; set; }

    public Color? FocusedForeground { get; set; }

    public Color? FocusedBorderColor { get; set; }

    public Color? PressedBackground { get; set; }

    public Color? PressedForeground { get; set; }

    public Color? PressedBorderColor { get; set; }
    
    public string? Text { get; set; }

    public bool LoseFocusAfterPress { get; set; } = false;
    
    public ImmutableList<ConsoleKey>? HandledKeys { get; set; }

    public ImmutableList<ConsoleKey> IgnoredKeys
    {
        get => _ignoredKeys;
        set => _ignoredKeys = value ?? throw new ArgumentNullException(nameof(value));
    }

    public BorderCharSet? BorderCharSet { get; set; }
    
    public BorderKind BorderKind { get; set; }

    public bool ShowPress { get; set; } = true;

    public HorizontalAligning TextHorizontalAligning { get; set; } = HorizontalAligning.Center;

    public VerticalAligning TextVerticalAligning { get; set; } = VerticalAligning.Center;

    public Size Size { get; }
    
    #endregion
    
    public Button Build(UIElementBuildArgs args)
    {
        int width = args.Width;
        int height = args.Height;

        var borderCharSet = BorderCharSet ?? (BorderKind == BorderKind.None ? null : BorderCharSets.Of(BorderKind));
        
        var resultInstance = new Button(width, height, Text, HandledKeys, IgnoredKeys, OverlappingPriority)
        {
            NotFocusedBackground = NotFocusedBackground,
            NotFocusedForeground = NotFocusedForeground,
            NotFocusedBorderColor = NotFocusedBorderColor,
            FocusedBackground = FocusedBackground ?? NotFocusedBackground,
            FocusedForeground = FocusedForeground ?? NotFocusedForeground,
            FocusedBorderColor = FocusedBorderColor ?? NotFocusedBorderColor,
            PressedBackground = PressedBackground ?? FocusedBackground ?? NotFocusedBackground,
            PressedForeground = PressedForeground ?? FocusedForeground ?? NotFocusedForeground,
            PressedBorderColor = PressedBorderColor ?? FocusedBorderColor ?? NotFocusedBorderColor,
            BorderCharSet = borderCharSet,
            LoseFocusAfterPress = LoseFocusAfterPress,
            ShowPress = ShowPress,
            TextHorizontalAligning = TextHorizontalAligning,
            TextVerticalAligning = TextVerticalAligning,
        };

        return resultInstance;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    #region Constructors
    
    public ButtonBuilder(int width, int height)
        : this(new Size(width, height))
    { }
    
    public ButtonBuilder(int width, double heightRelation)
        : this(new Size(width, heightRelation))
    { }
    
    public ButtonBuilder(double widthRelation, int height)
        : this(new Size(widthRelation, height))
    { }
    
    public ButtonBuilder(double widthRelation, double heightRelation)
        : this(new Size(widthRelation, heightRelation))
    { }
    
    public ButtonBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
    }

    #endregion
}