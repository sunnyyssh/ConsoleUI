using System.Diagnostics.CodeAnalysis;
using Sunnyyssh.ConsoleUI.Binding;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlock : UIElement
{
    private static readonly string DefaultText = string.Empty;

    private string? _text;

    private IObservable<string?, ValueChangedEventArgs<string?>>? _observing;

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
            
            Redraw(CreateDrawState());
        }
    }

    public void Observe(IObservable<string?, ValueChangedEventArgs<string?>> textObservable)
    {
        ArgumentNullException.ThrowIfNull(textObservable, nameof(textObservable));
        
        if (_observing is not null)
        {
            _observing.Updated -= HandleObservableTextUpdate;
        }

        _observing = textObservable;
        _observing.Updated += HandleObservableTextUpdate;
        Text = _observing.Value;
    }

    public void Unobserve()
    {
        if (_observing is null)
        {
            throw new InvalidOperationException("Nothing was observed.");
        }

        _observing.Updated -= HandleObservableTextUpdate;
        _observing = null;
        Text = null;
    }

    private void HandleObservableTextUpdate(IObservable<string?, UpdatedEventArgs> updated, UpdatedEventArgs args)
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