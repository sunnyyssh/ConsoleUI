namespace Sunnyyssh.ConsoleUI;

internal record LineChild(int Length, Orientation Orientation, int Left, int Top);

public sealed class LineComposition : UIElement
{
    private readonly LineChild[] _children;

    public LineCharSet CharSet { get; }

    public Color Color { get; init; }

    protected override DrawState CreateDrawState()
    {
        var stateBuilder = new DrawStateBuilder(Width, Height);

        var pixels = ResolvePixels(Width, Height);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var ch = ResolveChar(pixels[i, j]);

                if (ch is not null)
                {
                    stateBuilder[i, j] = new PixelInfo(ch.Value, Color.Transparent, Color);
                }
            }
        }

        return stateBuilder.ToDrawState();
    }

    private char? ResolveChar(Pixel pixel)
    {
        return pixel switch
        {
            { Left: true, Right: true, Down: true, Up: true } => CharSet.Cross,
            { Left: true, Right: true, Down: true } => CharSet.TopCorner,
            { Left: true, Right: true, Up: true } => CharSet.BottomCorner,
            { Left: true, Down: true, Up: true } => CharSet.RightCorner,
            { Right: true, Down: true, Up: true } => CharSet.LeftCorner,
            { Left: true, Down: true } => CharSet.RightTopCorner,
            { Right: true, Down: true } => CharSet.LeftTopCorner,
            { Left: true, Up: true } => CharSet.RightBottomCorner,
            { Right: true, Up: true } => CharSet.LeftBottomCorner,
            { Left: true } or { Right: true } => CharSet.HorizontalLine,
            { Up: true } or { Down: true } => CharSet.VerticalLine,
            _ => null
        };
    }

    private Pixel[,] ResolvePixels(int width, int height)
    {
        var resultPixels = new Pixel[width, height];

        foreach (var line in _children)
        {
            if (line.Orientation == Orientation.Horizontal)
            {
                resultPixels[line.Left, line.Top].Right = true;
                
                for (int i = 1; i < line.Length - 1; i++)
                {
                    resultPixels[line.Left + i, line.Top].Left = true;
                    resultPixels[line.Left + i, line.Top].Right = true;
                }

                resultPixels[line.Left + line.Length - 1, line.Top].Left = true;
                continue;
            }
            
            resultPixels[line.Left, line.Top].Down = true;
                
            for (int i = 1; i < line.Length - 1; i++)
            {
                resultPixels[line.Left, line.Top + i].Up = true;
                resultPixels[line.Left, line.Top + i].Down = true;
            }

            resultPixels[line.Left, line.Top + line.Length - 1].Up = true;
        }

        return resultPixels;
    }

    private struct Pixel
    {
        public bool Down;
        public bool Up;
        public bool Right;
        public bool Left;
    }

    private void ValidateChildren(LineChild[] children, int width, int height)
    {
        foreach (var child in children)
        {
            if (child.Left < 0 
                || child.Left + (child.Orientation == Orientation.Horizontal ? child.Length : 1) > width)
            {
                throw new ChildPlacementException("Can't place line at this position.");
            }
            
            if (child.Top < 0 
                || child.Top + (child.Orientation == Orientation.Vertical ? child.Length : 1) > height)
            {
                throw new ChildPlacementException("Can't place line at this position.");
            }
        }
    }
    
    internal LineComposition(int width, int height, LineChild[] children, LineCharSet charSet, OverlappingPriority priority) 
        : base(width, height, priority)
    {
        ArgumentNullException.ThrowIfNull(children, nameof(children));
        ValidateChildren(children, width, height);

        _children = children;
        
        CharSet = charSet;
    }
}