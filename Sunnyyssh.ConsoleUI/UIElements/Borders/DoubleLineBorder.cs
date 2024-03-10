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

    #region Constructors.
    
    public DoubleLineBorder(int width, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(width, height), color, priority)
    { }
    
    public DoubleLineBorder(int width, double heightRelation, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(width, heightRelation), color, priority)
    { }
    
    public DoubleLineBorder(double widthRelation, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(widthRelation, height), color, priority)
    { }
    
    public DoubleLineBorder(double widthRelation, double heightRelation, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(widthRelation, heightRelation), color, priority)
    { }

    public DoubleLineBorder(Size size, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : base(size, priority, DoubleLineCharSet, color)
    { }

    #endregion
}