using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;

public sealed class TextBlock : UIElement
{
    private static readonly string DefaultText = "";

    private string? _text;

    private IObservable<string>? _bound;
    
    public Color Background { get; init; }
    
    public Color Foreground { get; init; }

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
                Redraw(GetDrawState(ActualWidth, ActualHeight));
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
    
    protected override DrawState GetDrawState(int width, int height)
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

    #region Constructors

    public TextBlock(int width, int height, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : this(new Size(width, height))
    { }

    public TextBlock(int width, double heightRelation, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(width, heightRelation), priority)
    { }

    public TextBlock(double widthRelation, int height, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(widthRelation, height), priority)
    { }

    public TextBlock(double widthRelation, double heightRelation, OverlappingPriority priority = OverlappingPriority.Medium)
        : this(new Size(widthRelation, heightRelation), priority)
    { }
    
    public TextBlock(Size size, OverlappingPriority priority = OverlappingPriority.Medium) 
        : base(size, priority)
    {
        _text = DefaultText;
        Background = Color.Default;
        Foreground = Color.Default;
    }

    #endregion
}