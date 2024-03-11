﻿using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public class CharEnteredEventArgs : EventArgs
{
    public char Char { get; }
    
    public bool Backspace { get; }

    public CharEnteredEventArgs(char c, bool backspace)
    {
        Char = c;
        Backspace = backspace;
    }
}

public delegate void CharEnteredEventHandler(TextBox sender, CharEnteredEventArgs args);

public class TextEnteredEventArgs : EventArgs
{
    public string Text { get; }

    public TextEnteredEventArgs(string text)
    {
        Text = text;
    }
}

public delegate void TextEnteredEventHandler(TextBox sender, TextEnteredEventArgs args);

public sealed class TextBox : UIElement, IFocusable
{
    private static readonly string DefaultText = "";
    
    private string? _text;

    private IObservable<string>? _bound;

    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    private ForceLoseFocusHandler? _forceLoseFocusHandler;

    // CharEntered event should be handled less than in 5 ms.
    private readonly Handler<TextBox, CharEnteredEventArgs> _charEnteredHandler = new(5);

    // CharEntered event should be handled less than in 5 ms.
    private readonly Handler<TextBox, TextEnteredEventArgs> _textEnteredHandler = new(5);

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

    [NotNull]
    public string? Text
    {
        get => _text ?? DefaultText;
        set
        {
            var newText = CharHelper.RemoveSpecialChars(value, false);
            
            if (_text == newText)
                return;

            _text = newText;
            
            if (IsDrawn)
            {
                Redraw(GetDrawState(ActualWidth, ActualHeight));
            }
        }
    }

    public bool ShowPressedChars { get; set; } = true;

    public bool UserEditable { get; init; } = true;

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;

    public bool WordWrap { get; init; } = true;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Left;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Top;
    
    bool IFocusable.IsWaitingFocus => UserEditable;

    public bool IsFocused { get; private set; }

    protected override DrawState GetDrawState(int width, int height)
    {
        var builder = new DrawStateBuilder(width, height);
        
        builder.Fill(Background);
        
        if (BorderKind == BorderKind.None)
        {
            // Placing text in full area.
            PlaceText(0, 0, width, height, builder);
            return builder.ToDrawState();
        }
        
        Border.PlaceAt(0, 0, width, height, Background, BorderColor, BorderKind, builder);
        
        // Placing text in smaller area. (1 indent from each side).
        PlaceText(1, 1, width - 2, height - 2, builder);

        return builder.ToDrawState();
    }

    private void PlaceText(int left, int top, int width, int height, DrawStateBuilder builder)
    {
        var lines = TextHelper.SplitText(width, WordWrap, Text);

        int startingTop = top +
              (lines.Length >= height || TextVerticalAligning == VerticalAligning.Top
                  ? 0
                  : TextVerticalAligning == VerticalAligning.Center
                      ? (height - lines.Length) / 2
                      : height - lines.Length);
        
        int offset = Math.Max(lines.Length - height, 0);
        
        for (int i = 0; i < lines.Length - offset; i++)
        {
            string bareLine = lines[i + offset];
            string line = bareLine.Length > width ? bareLine[^width..] : bareLine;
            
            int lineLeft = left + TextHorizontalAligning switch
            {
                HorizontalAligning.Left => 0,
                HorizontalAligning.Center => (width - line.Length) / 2,
                HorizontalAligning.Right => width - line.Length,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            builder.Place(lineLeft, startingTop + i, line, Background, Foreground);
        }
    }

    public void Clear() => Text = null;

    public void Bind(IObservable<string> textObservable)
    {
        if (_bound is not null)
        {
            _bound.Updated -= HandleObservableTextUpdate;
        }

        _bound = textObservable;
        _bound.Updated += HandleObservableTextUpdate;
        Text = _bound.Value;
    }

    private void HandleObservableTextUpdate(IObservable<string> sender, UpdatedEventArgs args)
    {
        Text = sender.Value;
    }

    #region Key pressed handling.
    
    bool IFocusable.HandlePressedKey(ConsoleKeyInfo keyInfo)
    {
        if (IsSpecialKey(keyInfo.Key))
        {
            HandleSpecialKey(keyInfo.Key, out bool loseFocus);
            return !loseFocus;
        }
        
        char newChar = keyInfo.KeyChar;
        
        if (newChar == '\0')
            return true;
        
        if (ShowPressedChars)
        {
            Text += newChar;
        }
        
        OnCharEntered(newChar, false);
        
        return true;
    }

    private static readonly ConsoleKey[] SpecialKeys = new[] { ConsoleKey.Backspace, ConsoleKey.Enter };
    
    private bool IsSpecialKey(ConsoleKey key)
    {
        return SpecialKeys.Contains(key);
    }

    private void HandleSpecialKey(ConsoleKey key, out bool loseFocus)
    {
        switch (key)
        {
            case ConsoleKey.Enter:
                OnTextEntered(Text);
                
                loseFocus = true;
                return;
            
            case ConsoleKey.Backspace:
                if (Text.Length > 0)
                {
                    OnCharEntered('\r', true);
                    
                    if (ShowPressedChars)     
                        Text = Text[..^1];
                }
                
                loseFocus = false;
                return;
                
            default:
                throw new ArgumentException("Key wasn't special.", nameof(key));
        }
    }
    
    #endregion

    void IFocusable.TakeFocus()
    {
        IsFocused = true;
        
        Background = FocusedBackground;
        Foreground = FocusedForeground;
        BorderColor = FocusedBorderColor;

        if (IsDrawn)
        {
            Redraw(GetDrawState(ActualWidth, ActualHeight));
        }
    }

    void IFocusable.LoseFocus()
    {
        IsFocused = false;

        Background = NotFocusedBackground;
        Foreground = NotFocusedForeground;
        BorderColor = NotFocusedBorderColor;

        if (IsDrawn)
        {
            Redraw(GetDrawState(ActualWidth, ActualHeight));
        }
    }

    #region Events.
    
    private void OnCharEntered(char c, bool backspace) =>
        _charEnteredHandler.Invoke(this, new CharEnteredEventArgs(c, backspace));

    private void OnTextEntered(string text) => 
        _textEnteredHandler.Invoke(this, new TextEnteredEventArgs(text));

    public event CharEnteredEventHandler? CharEntered
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _charEnteredHandler.Add(
                new Action<TextBox, CharEnteredEventArgs>(value), 
                true);
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _charEnteredHandler.Remove(
                new Action<TextBox, CharEnteredEventArgs>(value));
        }
    }
    
    public event TextEnteredEventHandler? TextEntered
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _textEnteredHandler.Add(
                new Action<TextBox, TextEnteredEventArgs>(value), 
                true);
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _textEnteredHandler.Remove(
                new Action<TextBox, TextEnteredEventArgs>(value));
        }
    }

    event ForceTakeFocusHandler? IFocusable.ForceTakeFocus
    {
        add => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
    }

    event ForceLoseFocusHandler? IFocusable.ForceLoseFocus
    {
        add => _forceLoseFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceLoseFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }
    
    #endregion

    #region Constructors.
        
    public TextBox(int width, int height, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(width, height), priority)
    { }
        
    public TextBox(int width, double heightRelation, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(width, heightRelation), priority)
    { }

    public TextBox(double widthRelation, int height, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(widthRelation, height), priority)
    { }

    public TextBox(double widthRelation, double heightRelation, OverlappingPriority priority = OverlappingPriority.Medium) 
        : this(new Size(widthRelation, heightRelation), priority)
    { }

    public TextBox(Size size, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(size, priority)
    { }
        
    #endregion
}