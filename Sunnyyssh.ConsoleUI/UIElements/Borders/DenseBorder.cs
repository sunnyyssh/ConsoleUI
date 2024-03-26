namespace Sunnyyssh.ConsoleUI;

public sealed class DenseBorder : Border
{
    public static readonly BorderCharSet DenseCharSet = new BorderCharSet()
    {
        LeftVerticalLine = '\u2588',
        RightVerticalLine = '\u2588',
        TopHorizontalLine = '\u2580',
        BottomHorizontalLine = '\u2584',
        LeftTopCorner = '\u2588',
        RightTopCorner = '\u2588',
        LeftBottomCorner = '\u2588',
        RightBottomCorner = '\u2588'
    };

    public static void PlaceAt(int left, int top, int width, int height, Color background, Color foreground, DrawStateBuilder builder)
    {
        PlaceAt(left, top, width, height, background, foreground, DenseCharSet, builder);
    }

    public DenseBorder(int width, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : base(width, height, DenseCharSet, color, priority)
    { }
}