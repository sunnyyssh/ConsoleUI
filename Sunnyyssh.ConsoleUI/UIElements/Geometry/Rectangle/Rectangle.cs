namespace Sunnyyssh.ConsoleUI;

public sealed class Rectangle : UIElement
{
    private Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            
            if (IsDrawn)
            {
                Redraw(CreateDrawState(Width, Height));
            }
        }
    }

    protected override DrawState CreateDrawState(int width, int height)
    {
        var builder = new DrawStateBuilder(width, height);

        builder.Fill(Color);

        return builder.ToDrawState();
    }

    internal Rectangle(int width, int height, OverlappingPriority priority) 
        : base(width, height, priority)
    {
    }
}