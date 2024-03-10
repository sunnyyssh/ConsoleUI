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
    
    #region Constructors.
    
    public DenseBorder(int width, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(width, height), color, priority)
    { }
    
    public DenseBorder(int width, double heightRelation, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(width, heightRelation), color, priority)
    { }
    
    public DenseBorder(double widthRelation, int height, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(widthRelation, height), color, priority)
    { }
    
    public DenseBorder(double widthRelation, double heightRelation, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : this(new Size(widthRelation, heightRelation), color, priority)
    { }

    public DenseBorder(Size size, Color color, OverlappingPriority priority = OverlappingPriority.Lowest) 
        : base(size, priority, DenseCharSet, color)
    { }

    #endregion
}