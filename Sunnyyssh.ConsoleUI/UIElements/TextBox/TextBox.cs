// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Sunnyyssh.ConsoleUI.Binding;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sunnyyssh.ConsoleUI;

public sealed class CharEnteredEventArgs : EventArgs
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

public sealed class TextEnteredEventArgs : EventArgs
{
    public string Text { get; }

    public TextEnteredEventArgs(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        Text = text;
    }
}

public delegate void TextEnteredEventHandler(TextBox sender, TextEnteredEventArgs args);

public sealed class TextBox : UIElement, IFocusable
{
    private static readonly string DefaultText = string.Empty;
    
    private string? _text;

    private IObservable<string?, ValueChangedEventArgs<string?>>? _observing;

    private IBindable<string?, ValueChangedEventArgs<string?>>? _bound;

    private readonly Handler<TextBox, CharEnteredEventArgs> _charEnteredHandler = new(5);
    
    private readonly Handler<TextBox, TextEnteredEventArgs> _textEnteredHandler = new(5);
    
    // ReSharper disable once NotAccessedField.Local
    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    // ReSharper disable once NotAccessedField.Local
    private ForceLoseFocusHandler? _forceLoseFocusHandler;

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
            SetText(value);
            _bound?.HandleUpdate(new ValueChangedEventArgs<string?>(_text));
        }
    }

    public bool IsWaitingFocus { get; set; } = true;
    
    public bool ShowPressedChars { get; init; } = true;

    public bool UserEditable { get; init; } = true;

    public BorderKind BorderKind { get; init; } = BorderKind.SingleLine;

    public bool WordWrap { get; init; } = true;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Left;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Top;
    
    bool IFocusable.IsWaitingFocus => UserEditable && IsWaitingFocus;

    public bool IsFocused { get; private set; }

    public void RegisterCharEnteredHandler(CharEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _charEnteredHandler.Add(new Action<TextBox, CharEnteredEventArgs>(handler), true);
    }

    public void UnsafeRegisterCharEnteredHandler(CharEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _charEnteredHandler.Add(new Action<TextBox, CharEnteredEventArgs>(handler), false);
    }

    public void RemoveCharEnteredHandler(CharEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _charEnteredHandler.Remove(new Action<TextBox, CharEnteredEventArgs>(handler));
    }

    public void RegisterTextEnteredHandler(TextEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _textEnteredHandler.Add(new Action<TextBox, TextEnteredEventArgs>(handler), true);
    }
    
    public void UnsafeRegisterTextEnteredHandler(TextEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _textEnteredHandler.Add(new Action<TextBox, TextEnteredEventArgs>(handler), false);
    }
    
    public void RemoveTextEnteredHandler(TextEnteredEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _textEnteredHandler.Remove(new Action<TextBox, TextEnteredEventArgs>(handler));
    }
    
    protected override DrawState CreateDrawState()
    {
        var builder = new DrawStateBuilder(Width, Height);
        
        builder.Fill(Background);
        
        if (BorderKind == BorderKind.None)
        {
            // Placing text in full area.
            TextHelper.PlaceText(0, 0, Width, Height,
                WordWrap, Text, Background, Foreground,
                TextVerticalAligning, TextHorizontalAligning, builder);
            return builder.ToDrawState();
        }
        
        Border.PlaceAt(0, 0, Width, Height, 
            Background, BorderColor, BorderKind, builder);
        
        // Placing text in smaller area. (1 indent from each side).
        TextHelper.PlaceText(1, 1, Width - 2, Height - 2,
            WordWrap, Text, Background, Foreground,
            TextVerticalAligning, TextHorizontalAligning, builder);

        return builder.ToDrawState();
    }

    public void Clear() => Text = null;

    public void Observe(IObservable<string?, ValueChangedEventArgs<string?>> textObservable)
    {
        ArgumentNullException.ThrowIfNull(textObservable, nameof(textObservable));
        
        if (UserEditable)
        {
            throw new InvalidOperationException($"Can't observe {nameof(textObservable)} when {UserEditable}=true." +
                                                $"Try {nameof(Bind)} or set {nameof(UserEditable)}=false.");
        }
        
        if (_observing is not null)
        {
            _observing.Updated -= HandleObservableTextUpdate;
        }

        _observing = textObservable;
        _observing.Updated += HandleObservableTextUpdate;
        SetText(_observing.Value);
    }

    public void Unobserve()
    {
        if (_observing is null)
        {
            throw new InvalidOperationException("Nothing was observed.");
        }

        _observing.Updated -= HandleObservableTextUpdate;
        _observing = null;
        SetText(null);
    }

    public void Bind(IBindable<string?, ValueChangedEventArgs<string?>> textBindable)
    {
        ArgumentNullException.ThrowIfNull(textBindable, nameof(textBindable));
        
        if (_observing is not null)
        {
            _observing.Updated -= HandleObservableTextUpdate;
        }

        _observing = textBindable;
        _observing.Updated += HandleObservableTextUpdate;
        
        SetText(_observing.Value);
        
        _bound = textBindable;
    }

    public void Unbind()
    {
        if (_observing is null || _bound is null)
        {
            throw new InvalidOperationException("Nothing was bound.");
        }

        _observing.Updated -= HandleObservableTextUpdate;
        _observing = null;
        
        _bound = null;

        SetText(null);
    }

    private void SetText(string? value)
    {
        var newText = CharHelper.RemoveSpecialChars(value, false);
            
        if (_text == newText)
            return;

        _text = newText;
            
        if (IsStateInitialized)
        {
            Redraw(CreateDrawState());
        }
    }
    
    private void HandleObservableTextUpdate(IObservable<string?, ValueChangedEventArgs<string?>> sender, ValueChangedEventArgs<string?> args)
    {
        SetText(args.NewValue);
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

        if (IsStateInitialized)
        {
            Redraw(CreateDrawState());
        }
    }

    void IFocusable.LoseFocus()
    {
        IsFocused = false;

        Background = NotFocusedBackground;
        Foreground = NotFocusedForeground;
        BorderColor = NotFocusedBorderColor;

        if (IsStateInitialized)
        {
            Redraw(CreateDrawState());
        }
    }

    #region Events.
    
    private void OnCharEntered(char c, bool backspace) =>
        _charEnteredHandler.Invoke(this, new CharEnteredEventArgs(c, backspace));

    private void OnTextEntered(string text) => 
        _textEnteredHandler.Invoke(this, new TextEnteredEventArgs(text));

    public event CharEnteredEventHandler CharEntered
    {
        add => RegisterCharEnteredHandler(value);
        remove => RemoveCharEnteredHandler(value);
    }

    public event TextEnteredEventHandler TextEntered
    {
        add => RegisterTextEnteredHandler(value);
        remove => RemoveTextEnteredHandler(value);
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
    
    #endregion

    internal TextBox(int width, int height, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(width, height, priority)
    { }
}