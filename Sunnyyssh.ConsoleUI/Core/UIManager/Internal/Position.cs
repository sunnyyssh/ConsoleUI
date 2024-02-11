using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

[Flags]
public enum Positioning
{
    Absolute,
    RelationalLeft,
    RelationalTop,
    Relational = RelationalLeft | RelationalTop,
}

public class Position
{
    public int? Left { get; }
    public int? Top { get; }
    public double? LeftRelational { get; }
    public double? TopRelational { get; }
    public Positioning Positioning { get; }

    [MemberNotNullWhen(true, nameof(LeftRelational))]
    [MemberNotNullWhen(false, nameof(Left))]
    public bool IsLeftRelational => Positioning.HasFlag(Positioning.RelationalLeft);
    
    [MemberNotNullWhen(true, nameof(TopRelational))]
    [MemberNotNullWhen(false, nameof(Top))]
    public bool IsTopRelational => Positioning.HasFlag(Positioning.RelationalTop);
    
    public Position(Positioning positioning, int? left, int? top, double? leftRelational, double? topRelational)
    {
        if (positioning.HasFlag(Positioning.RelationalLeft))
            ArgumentNullException.ThrowIfNull(leftRelational, nameof(leftRelational));
        else
            ArgumentNullException.ThrowIfNull(left, nameof(left));
        
        if (positioning.HasFlag(Positioning.RelationalTop))
            ArgumentNullException.ThrowIfNull(topRelational, nameof(topRelational));
        else
            ArgumentNullException.ThrowIfNull(top, nameof(top));
        
        Positioning = positioning;
        Left = left;
        Top = top;
        LeftRelational = leftRelational;
        TopRelational = topRelational;
    }
}