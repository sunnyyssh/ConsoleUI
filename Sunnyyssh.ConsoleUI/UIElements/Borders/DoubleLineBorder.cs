namespace Sunnyyssh.ConsoleUI;

public sealed class DoubleLineBorder : Border
{
    public static readonly BorderCharSet DoubleLineCharSet = new BorderCharSet()
    {
        LeftVerticalLine = '\u2551',
        RightVerticalLine = '\u2551',
        TopHorizontalLine = '\u2550',
        BottomHorizontalLine = '\u2550',
        LeftTopCorner = '\u2554',
        RightTopCorner = '\u2557',
        LeftBottomCorner = '\u255a',
        RightBottomCorner = '\u255d'
    };

    public static void PlaceAt(int left, int top, int width, int height, Color background, Color foreground, DrawStateBuilder builder)
    {
        PlaceAt(left, top, width, height, background, foreground, DoubleLineCharSet, builder);
    }

    internal DoubleLineBorder(int width, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : base(width, height, DoubleLineCharSet, color, priority)
    { }
}