using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class ButtonBuilder : IUIElementBuilder<Button>
{
    private ButtonPressedHandler? _safeButtonPressedHandler;

    private ButtonPressedHandler? _unsafeButtonPressedHandler;
    
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

    #region Chain setters.

    public ButtonBuilder SetOverlappingPriority(OverlappingPriority priority)
    {
        OverlappingPriority = priority;
        return this;
    }
    
    public ButtonBuilder SetNotFocusedBackground(Color color)
    {
        NotFocusedBackground = color;
        return this;
    }

    public ButtonBuilder SetNotFocusedForeground(Color color)
    {
        NotFocusedForeground = color;
        return this;
    }

    public ButtonBuilder SetNotFocusedBorderColor(Color color)
    {
        NotFocusedBorderColor = color;
        return this;
    }

    public ButtonBuilder SetFocusedBackground(Color color)
    {
        FocusedBackground = color;
        return this;
    }

    public ButtonBuilder SetFocusedForeground(Color color)
    {
        FocusedForeground = color;
        return this;
    }

    public ButtonBuilder SetFocusedBorderColor(Color color)
    {
        FocusedBorderColor = color;
        return this;
    }
    
    public ButtonBuilder SetPressedBackground(Color color)
    {
        PressedBackground = color;
        return this;
    }

    public ButtonBuilder SetPressedForeground(Color color)
    {
        PressedForeground = color;
        return this;
    }

    public ButtonBuilder SetPressedBorderColor(Color color)
    {
        PressedBorderColor = color;
        return this;
    }

    public ButtonBuilder SetText(string text)
    {
        Text = text;
        return this;
    }

    public ButtonBuilder SetLoseFocusAfterPress(bool loseFocus)
    {
        LoseFocusAfterPress = loseFocus;
        return this;
    }

    public ButtonBuilder SetHandledKeys(ImmutableList<ConsoleKey>? handledKeys)
    {
        HandledKeys = handledKeys;
        return this;
    }

    public ButtonBuilder SetIgnoredKeys(ImmutableList<ConsoleKey> ignoredKeys)
    {
        IgnoredKeys = ignoredKeys;
        return this;
    }

    public ButtonBuilder SetBorderCharSet(BorderCharSet? charSet)
    {
        BorderCharSet = charSet;
        return this;
    }

    public ButtonBuilder SetBorderKind(BorderKind borderKind)
    {
        BorderKind = borderKind;
        return this;
    }

    public ButtonBuilder SetShowPress(bool show)
    {
        ShowPress = show;
        return this;
    }

    public ButtonBuilder SetTextHorizontalAligning(HorizontalAligning aligning)
    {
        TextHorizontalAligning = aligning;
        return this;
    }
    
    public ButtonBuilder SetTextVerticalAligning(VerticalAligning aligning)
    {
        TextVerticalAligning = aligning;
        return this;
    }

    public ButtonBuilder RegisterPressedHandler(ButtonPressedHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _safeButtonPressedHandler += handler;
        return this;
    }

    public ButtonBuilder UnsafeRegisterPressedHandler(ButtonPressedHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _unsafeButtonPressedHandler += handler;
        return this;
    }
    
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

        if (_safeButtonPressedHandler is not null)
        {
            resultInstance.RegisterPressedHandler(_safeButtonPressedHandler);
        }

        if (_unsafeButtonPressedHandler is not null)
        {
            resultInstance.UnsafeRegisterPressedHandler(_unsafeButtonPressedHandler);
        }

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