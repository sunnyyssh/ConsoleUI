using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlock : UIElement
{
    private static readonly string DefaultText = "";

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

    #region Draw state handling.
    
    protected override DrawState CreateDrawState(int width, int height)
    {
        var builder = new DrawStateBuilder(width, height);
        
        builder.Fill(Background);
        PlaceText(0, 0, width, height, builder);

        return builder.ToDrawState();
    }

    private void PlaceText(int left, int top, int width, int height, DrawStateBuilder builder)
    {
        var lines = TextHelper.SplitText(width, WordWrap, Text);

        int startingTop = lines.Length >= height || TextVerticalAligning == VerticalAligning.Top 
            ? 0
            : TextVerticalAligning == VerticalAligning.Center 
                ? (height - lines.Length) / 2
                : height - lines.Length;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (startingTop + i >= height)
                break;

            string line = lines[i].Length > width ? lines[i][..width] : lines[i];
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
    
    #endregion
    
    public TextBlock(int width, int height, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(width, height, priority)
    { }
}