﻿using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlock : UIElement
{
    private static readonly string DefaultText = string.Empty;

    private string? _text;

    private IObservable<string, UpdatedEventArgs>? _bound;

    public Color Background { get; init; } = Color.Default;

    public Color Foreground { get; init; } = Color.Default;

    public VerticalAligning TextVerticalAligning { get; init; } = VerticalAligning.Center;

    public HorizontalAligning TextHorizontalAligning { get; init; } = HorizontalAligning.Center;

    public bool WordWrap { get; init; } = true;

    [NotNull]
    public string? Text
    {
        get => _text ?? DefaultText;
        set
        {
            _text = value;
            
            if (IsDrawn)
            {
                Redraw(CreateDrawState());
            }
        }
    }

    public void Bind(IObservable<string, UpdatedEventArgs> textObservable)
    {
        ArgumentNullException.ThrowIfNull(textObservable, nameof(textObservable));
        
        if (_bound is not null)
        {
            _bound.Updated -= HandleTextUpdate;
        }

        _bound = textObservable;
        _bound.Updated += HandleTextUpdate;
        Text = _bound.Value;
    }

    private void HandleTextUpdate(IObservable<string, UpdatedEventArgs> updated, UpdatedEventArgs args)
    {
        Text = updated.Value;
    }
    
    protected override DrawState CreateDrawState()
    {
        var builder = new DrawStateBuilder(Width, Height);
        
        builder.Fill(Background);
        
        TextHelper.PlaceText(0, 0, Width, Height, 
            WordWrap, Text, Background, Foreground, 
            TextVerticalAligning, TextHorizontalAligning, builder);

        return builder.ToDrawState();
    }
    
    internal TextBlock(int width, int height, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(width, height, priority)
    { }
}