using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlock : UIElement
{
    private static readonly string DefaultText = string.Empty;

    private string? _text;

    private IObservable<string>? _bound;

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
                Redraw(CreateDrawState(Width, Height));
            }
        }
    }

    public void Bind(IObservable<string> textObservable)
    {
        if (_bound is not null)
        {
            _bound.Updated -= HandleTextUpdate;
        }

        _bound = textObservable;
        _bound.Updated += HandleTextUpdate;
        Text = _bound.Value;
    }

    private void HandleTextUpdate(IObservable<string> updated, UpdatedEventArgs args)
    {
        Text = updated.Value;
    }
    
    protected override DrawState CreateDrawState(int width, int height)
    {
        var builder = new DrawStateBuilder(width, height);
        
        builder.Fill(Background);
        
        TextHelper.PlaceText(0, 0, width, height, 
            WordWrap, Text, Background, Foreground, 
            TextVerticalAligning, TextHorizontalAligning, builder);

        return builder.ToDrawState();
    }
    
    internal TextBlock(int width, int height, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(width, height, priority)
    { }
}