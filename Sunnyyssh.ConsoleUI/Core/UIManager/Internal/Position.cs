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

public class Position // TODO implememnt an ability to take absolute offset to relation.
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

    public Position(int left, int top)
        : this(Positioning.Absolute, left, top, null, null)
    { }

    public Position(int left, double topRelational)
        : this(Positioning.RelationalTop, left, null, null, topRelational)
    { }

    public Position(double leftRelational, int top)
        : this(Positioning.RelationalLeft, null, top, leftRelational, null)
    { }

    public Position(double leftRelational, double topRelational)
        : this(Positioning.Relational, null, null, leftRelational, topRelational)
    { }

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