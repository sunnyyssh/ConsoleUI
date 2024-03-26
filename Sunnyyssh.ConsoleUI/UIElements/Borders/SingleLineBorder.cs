namespace Sunnyyssh.ConsoleUI;

public sealed class SingleLineBorder : Border
{
    public static readonly BorderCharSet SingleLineCharSet = new BorderCharSet()
    {
        LeftVerticalLine = '\u2502',
        RightVerticalLine = '\u2502',
        TopHorizontalLine = '\u2500',
        BottomHorizontalLine = '\u2500',
        LeftTopCorner = '\u250c',
        RightTopCorner = '\u2510',
        LeftBottomCorner = '\u2514',
        RightBottomCorner = '\u2518'
    };

    public static void PlaceAt(int left, int top, int width, int height, Color background, Color foreground, DrawStateBuilder builder)
    {
        PlaceAt(left, top, width, height, background, foreground, SingleLineCharSet, builder);
    }
    public SingleLineBorder(int width, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : base(width ,height, SingleLineCharSet, color, priority)
    { }
}