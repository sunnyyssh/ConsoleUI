using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public record ButtonPressedArgs(ConsoleKeyInfo PressedKey);

public delegate void ButtonPressedHandler(Button sender, ButtonPressedArgs args);

public sealed class Button : UIElement, IFocusable
{
    private const int ShowPressedDelayMilliseconds = 20;
    
    private ForceTakeFocusHandler? _forceTakeFocusHandler;
    
    private ForceLoseFocusHandler? _forceLoseFocusHandler;

    private readonly Handler<Button, ButtonPressedArgs> _pressedHandler = new(5);

    private int _showingPressesCount = 0;
    
    #region Colors.

    private readonly Color? _pressedBackground;
    
    private readonly Color? _pressedForeground;
    
    private readonly Color? _pressedBorderColor;

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

    public Color PressedBackground
    {
        get => _pressedBackground ?? FocusedBackground;
        init => _pressedBackground = value;
    }

    public Color PressedForeground
    {
        get => _pressedForeground ?? FocusedForeground;
        init => _pressedForeground = value;
    }

    public Color PressedBorderColor
    {
        get => _pressedBorderColor ?? FocusedBorderColor;
        init => _pressedBorderColor = value;
    }

    #endregion 
    
    public BorderCharSet? BorderCharSet { get; init; }

    [MemberNotNullWhen(true, nameof(BorderCharSet))]
    public bool HasBorder => BorderCharSet is not null;
    
    public string Text { get; }

    public bool LoseFocusAfterPress { get; init; } = false;
    
    public ConsoleKeyCollection? HandledKeys { get; }
    
    public ConsoleKeyCollection IgnoredKeys { get; }

    public bool ShowPress { get; init; } = true;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Center;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Center;

    public bool IsWaitingFocus { get; set; } = true;

    public bool IsFocused { get; private set; }

    public void UnsafeRegisterPressedHandler(ButtonPressedHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _pressedHandler.Add(new Action<Button, ButtonPressedArgs>(handler), false);
    }
    
    public void RegisterPressedHandler(ButtonPressedHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _pressedHandler.Add(new Action<Button, ButtonPressedArgs>(handler), true);
    }

    public void RemovePressedHandler(ButtonPressedHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _pressedHandler.Remove(new Action<Button, ButtonPressedArgs>(handler));
    }

    protected override DrawState CreateDrawState()
    {
        var builder = new DrawStateBuilder(Width, Height);
        
        builder.Fill(Background);
        
        if (!HasBorder)
        {
            // Placing text in full area.
            TextHelper.PlaceText(0, 0, Width, Height,
                false, Text, Background, Foreground,
                TextVerticalAligning, TextHorizontalAligning, builder);
            return builder.ToDrawState();
        }
        
        Border.PlaceAt(0, 0, Width, Height, 
            Background, BorderColor, BorderCharSet, builder);
        
        // Placing text in smaller area. (1 indent from each side).
        TextHelper.PlaceText(1, 1, Width - 2, Height - 2,
            false, Text, Background, Foreground,
            TextVerticalAligning, TextHorizontalAligning, builder);

        return builder.ToDrawState();
    }

    bool IFocusable.HandlePressedKey(ConsoleKeyInfo keyInfo)
    {
        var key = keyInfo.Key;
        
        if (IgnoredKeys.Contains(key))
        {
            return true;
        }

        if (HandledKeys is not null && !HandledKeys.Contains(key))
        {
            return true;
        }

        if (ShowPress)
        {
            ShowPressDelayed();
        }
        
        OnPressed(keyInfo);

        bool keepFocus = !LoseFocusAfterPress;
        return keepFocus;
    }

    private void ShowPressDelayed()
    {
        Background = PressedBackground;
        Foreground = PressedForeground;
        BorderColor = PressedBorderColor;

        Interlocked.Increment(ref _showingPressesCount);

        Redraw(CreateDrawState());

        Task.Delay(ShowPressedDelayMilliseconds)
            .ContinueWith(ResetStateAfterShowPress);

        void ResetStateAfterShowPress(Task obj)
        {
            if (IsFocused)
            {
                Background = FocusedBackground;
                Foreground = FocusedForeground;
                BorderColor = FocusedBorderColor;
            }
            else
            {
                Background = NotFocusedBackground;
                Foreground = NotFocusedForeground;
                BorderColor = NotFocusedBorderColor;
            }

            if (Interlocked.Decrement(ref _showingPressesCount) == 0 && IsDrawn)
            {
                Redraw(CreateDrawState());
            }
        }
    }


    void IFocusable.TakeFocus()
    {
        IsFocused = true;

        // if it's showing press we mustn't redraw. It will do it on its own.
        if (_showingPressesCount > 0)
        {
            return;
        }
        
        Background = FocusedBackground;
        Foreground = FocusedForeground;
        BorderColor = FocusedBorderColor;

        if (IsDrawn)
        {
            Redraw(CreateDrawState());
        }
    }

    void IFocusable.LoseFocus()
    {
        IsFocused = false;

        // if it's showing press we mustn't redraw. It will do it on its own.
        if (_showingPressesCount > 0)
        {
            return;
        }

        Background = NotFocusedBackground;
        Foreground = NotFocusedForeground;
        BorderColor = NotFocusedBorderColor;

        if (IsDrawn)
        {
            Redraw(CreateDrawState());
        }
    }

    private void OnPressed(ConsoleKeyInfo pressedKey)
    {
        var args = new ButtonPressedArgs(pressedKey);
        
        _pressedHandler.Invoke(this, args);
    }
    
    public event ButtonPressedHandler Pressed
    {
        add => RegisterPressedHandler(value);
        remove => RemovePressedHandler(value);
    }

    event ForceTakeFocusHandler IFocusable.ForceTakeFocus
    {
        add => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
    }

    event ForceLoseFocusHandler IFocusable.ForceLoseFocus
    {
        add => _forceLoseFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceLoseFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="text"></param>
    /// <param name="handledKeys">Keys than should be detected as button press. null if all keys should be detected.</param>
    /// <param name="ignoredKeys">Keys that should be ignored. Excludes from <see cref="handledKeys"/>.</param>
    /// <param name="priority"></param>
    internal Button(int width, int height, string? text, 
        ConsoleKeyCollection? handledKeys, ConsoleKeyCollection ignoredKeys, 
        OverlappingPriority priority) 
        : base(width, height, priority)
    {
        ArgumentNullException.ThrowIfNull(ignoredKeys, nameof(ignoredKeys));

        Text = text ?? string.Empty;
        HandledKeys = handledKeys;
        IgnoredKeys = ignoredKeys;
    }
}