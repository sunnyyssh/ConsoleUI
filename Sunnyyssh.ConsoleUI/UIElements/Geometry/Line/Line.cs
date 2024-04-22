// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public sealed class Line : UIElement
{
    public Orientation Orientation { get; }
    
    public LineCharSet CharSet { get; }

    public Color Color { get; init; } = Color.Default;
    
    public int Length { get; }

    protected override DrawState CreateDrawState()
    {
        var builder = new DrawStateBuilder(Width, Height);
        
        char ch = Orientation == Orientation.Vertical
            ? CharSet.VerticalLine
            : CharSet.HorizontalLine;

        builder.Fill(new PixelInfo(ch, Color.Transparent, Color));

        return builder.ToDrawState();
    }

    private static int ResolveWidth(int length, Orientation orientation)
    {
        if (orientation == Orientation.Vertical)
        {
            return 1;
        }

        return length;
    }

    private static int ResolveHeight(int length, Orientation orientation)
    {
        if (orientation == Orientation.Horizontal)
        {
            return 1;
        }

        return length;
    }

    internal Line(int length, Orientation orientation, LineKind kind, OverlappingPriority priority)
        : this(length, orientation, LineCharSets.Of(kind), priority)
    {
        
    }

    internal Line(int length, Orientation orientation, LineCharSet charSet, OverlappingPriority priority) 
        : base(ResolveWidth(length, orientation), 
            ResolveHeight(length, orientation), 
            priority)
    {
        ArgumentNullException.ThrowIfNull(charSet, nameof(charSet));
        
        Length = length;
        Orientation = orientation;
        CharSet = charSet;
    }
}