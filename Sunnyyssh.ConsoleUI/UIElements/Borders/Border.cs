namespace Sunnyyssh.ConsoleUI;

public abstract class Border : UIElement
{
    protected BorderCharSet CharSet { get; }
    
    public Color Color { get; }

    protected override DrawState CreateDrawState()
    {
        var builder = new DrawStateBuilder(Width, Height);
        
        PlaceAt(0, 0, Width, Height, 
            Color.Transparent, Color, 
            CharSet, builder);

        return builder.ToDrawState();
    }

    public static void PlaceAt(int left, int top, int width, int height, 
        Color background, Color foreground, BorderKind borderKind, DrawStateBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        var charSet = borderKind == BorderKind.None ? null : BorderCharSets.Of(borderKind);
        
        if (charSet is null)
            return;
        
        PlaceAt(left, top, width, height, background, foreground, charSet, builder);
    }

    public static void PlaceAt(int left, int top, int width, int height, 
        Color background, Color foreground, BorderCharSet charSet, DrawStateBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(charSet, nameof(charSet));
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        if (left < 0 || left >= builder.Width)
            throw new ArgumentOutOfRangeException(nameof(left));
        if (top < 0 || top >= builder.Height)
            throw new ArgumentOutOfRangeException(nameof(top));
        if (width <= 0 || left + width > builder.Width)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0 || top + height > builder.Height)
            throw new ArgumentOutOfRangeException(nameof(height));
        
        // Setting corners.
        builder[left, top] = new PixelInfo(charSet.LeftTopCorner, 
            background, foreground);
        
        builder[left + width - 1, top] = new PixelInfo(charSet.RightTopCorner, 
            background, foreground);
        
        builder[left, top + height - 1] = new PixelInfo(charSet.LeftBottomCorner,
            background, foreground);
        
        builder[left + width - 1, top + height - 1] = new PixelInfo(charSet.RightBottomCorner,
            background, foreground);
        
        // Setting horizontal lines.
        builder.Fill(left + 1, top, width - 2, 1, // Top line.
            new PixelInfo(charSet.TopHorizontalLine, 
                background, foreground));
        
        builder.Fill(left + 1, top + height - 1, width - 2, 1, // Bottom line.
            new PixelInfo(charSet.BottomHorizontalLine, 
                background, foreground));
        
        // Setting vertical lines.
        builder.Fill(left, top + 1, 1, height - 2, // Left line.
            new PixelInfo(charSet.LeftVerticalLine, 
                background, foreground));
        
        builder.Fill(left + width - 1, top + 1, 1, height - 2, // Right line.
            new PixelInfo(charSet.RightVerticalLine, 
                background, foreground));
    }

    protected Border(int width, int height, BorderCharSet charSet, Color color, OverlappingPriority priority) 
        : base(width, height, priority)
    {
        ArgumentNullException.ThrowIfNull(charSet, nameof(charSet));

        CharSet = charSet;
        Color = color;
    }
}