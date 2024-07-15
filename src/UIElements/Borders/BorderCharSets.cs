// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public static class BorderCharSets
{
    public static readonly BorderCharSet SingleLine = new BorderCharSet()
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
    
    public static readonly BorderCharSet DoubleLine = new BorderCharSet()
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
    
    public static readonly BorderCharSet Dense = new BorderCharSet()
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

    public static BorderCharSet Of(BorderKind borderKind)
    {
        return borderKind switch
        {
            BorderKind.SingleLine => SingleLine,
            BorderKind.DoubleLine => DoubleLine,
            BorderKind.Dense => Dense,
            _ => throw new ArgumentOutOfRangeException(nameof(borderKind), borderKind, null)
        };
    }
}