namespace Sunnyyssh.ConsoleUI;

public enum LineKind
{
    Single,
    Double,
    Dense,
}

public static class LineCharSets
{
    public static readonly LineCharSet SingleLine = new LineCharSet
    {
        VerticalLine = '\u2502',
        HorizontalLine = '\u2500',
        LeftCorner = '\u251c',
        RightCorner = '\u2524',
        TopCorner = '\u252c',
        BottomCorner = '\u2534',
        LeftTopCorner = '\u250c',
        RightTopCorner = '\u2510',
        LeftBottomCorner = '\u2514',
        RightBottomCorner = '\u2518',
        Cross = '\u253c'
    };
    
    public static readonly LineCharSet DoubleLine = new LineCharSet
    {
        VerticalLine = '\u2551',
        HorizontalLine = '\u2550',
        LeftCorner = '\u2560',
        RightCorner = '\u2563',
        TopCorner = '\u2566',
        BottomCorner = '\u2569',
        LeftTopCorner = '\u2554',
        RightTopCorner = '\u2557',
        LeftBottomCorner = '\u255a',
        RightBottomCorner = '\u255d',
        Cross = '\u256c'
    };
    
    public static readonly LineCharSet DenseLine = new LineCharSet
    {
        VerticalLine = '\u2588',
        HorizontalLine = '\u2588',
        LeftCorner = '\u2588',
        RightCorner = '\u2588',
        TopCorner = '\u2588',
        BottomCorner = '\u2588',
        LeftTopCorner = '\u2588',
        RightTopCorner = '\u2588',
        LeftBottomCorner = '\u2588',
        RightBottomCorner = '\u2588',
        Cross = '\u2588'
    };

    public static LineCharSet Of(LineKind kind)
    {
        return kind switch
        {
            LineKind.Single => SingleLine,
            LineKind.Double => DoubleLine,
            LineKind.Dense => DenseLine,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "this kind of line is not implemented.")
        };
    }

    public static LineCharSet? Of(BorderKind kind)
    {
        return kind switch
        {
            BorderKind.None => null,
            BorderKind.SingleLine => SingleLine,
            BorderKind.DoubleLine => DoubleLine,
            BorderKind.Dense => DenseLine,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "this kind of border is not implemented.")
        };
    }
}