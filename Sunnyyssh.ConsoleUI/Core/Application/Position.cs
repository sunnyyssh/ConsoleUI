using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

[Flags]
internal enum Positioning
{
    Absolute,
    RelationalLeft,
    RelationalTop,
    Relational = RelationalLeft | RelationalTop,
}

/// <summary>
/// Represents the position.
/// </summary>
public class Position
{
    /// <summary>
    /// The leftmost topmost position. <code>= new Position(0, 0)</code>
    /// </summary>
    public static readonly Position LeftTop = new Position(0, 0);
    
    /// <summary>
    /// Left absolute position. (Counted in characters).
    /// </summary>
    public int? Left { get; }
    
    /// <summary>
    /// Top absolute position. (Counted in characters).
    /// </summary>
    public int? Top { get; }
    
    /// <summary>
    /// Left relational position. (Counted from the whole width and cen be more than or equal to 0 and less than 1).
    /// </summary>
    public double? LeftRelational { get; }
    
    /// <summary>
    /// Top relational position. (Counted from the whole height and cen be more than or equal to 0 and less than 1).
    /// </summary>
    public double? TopRelational { get; }
    
    /// <summary>
    /// The type of position.
    /// </summary>
    internal Positioning Positioning { get; }

    /// <summary>
    /// Indicates if left position is relational. If it's false then left position is absolute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(LeftRelational))]
    [MemberNotNullWhen(false, nameof(Left))]
    public bool IsLeftRelational => Positioning.HasFlag(Positioning.RelationalLeft);

    /// <summary>
    /// Indicates if top position is relational. If it's false then top position is absolute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(TopRelational))]
    [MemberNotNullWhen(false, nameof(Top))]
    public bool IsTopRelational => Positioning.HasFlag(Positioning.RelationalTop);

    /// <summary>
    /// Creates <see cref="Position"/> with absolute <see cref="left"/> and absolute <see cref="top"/>.
    /// </summary>
    /// <param name="left">Absolute left position.</param>
    /// <param name="top">Absolute top position.</param>
    public Position(int left, int top)
        : this(Positioning.Absolute, left, top, null, null)
    { }
    
    /// <summary>
    /// Creates <see cref="Position"/> with absolute <see cref="left"/> and relational <see cref="TopRelational"/>.
    /// </summary>
    /// <param name="left">Absolute left position.</param>
    /// <param name="topRelational">Relational top position.</param>
    public Position(int left, double topRelational)
        : this(Positioning.RelationalTop, left, null, null, topRelational)
    { }

    /// <summary>
    /// Creates <see cref="Position"/> with relational <see cref="leftRelational"/> and absolute <see cref="top"/>.
    /// </summary>
    /// <param name="leftRelational">Relational left position.</param>
    /// <param name="top">Absolute top position.</param>
    public Position(double leftRelational, int top)
        : this(Positioning.RelationalLeft, null, top, leftRelational, null)
    { }

    /// <summary>
    /// Creates <see cref="Position"/> with relational <see cref="LeftRelational"/> and relational <see cref="TopRelational"/>.
    /// </summary>
    /// <param name="leftRelational">Relational left position.</param>
    /// <param name="topRelational">Relational top position.</param>
    public Position(double leftRelational, double topRelational)
        : this(Positioning.Relational, null, null, leftRelational, topRelational)
    { }

    private Position(Positioning positioning, int? left, int? top, double? leftRelational, double? topRelational)
    {
        if (positioning.HasFlag(Positioning.RelationalLeft))
            ArgumentNullException.ThrowIfNull(leftRelational, nameof(leftRelational));
        else
            ArgumentNullException.ThrowIfNull(left, nameof(left));

        if (positioning.HasFlag(Positioning.RelationalTop))
            ArgumentNullException.ThrowIfNull(topRelational, nameof(topRelational));
        else
            ArgumentNullException.ThrowIfNull(top, nameof(top));

        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), left, null);
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(left), left, null);
        if (leftRelational < 0.0 || leftRelational >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(leftRelational), leftRelational, null);
        if (topRelational < 0.0 || topRelational >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(topRelational), topRelational, null);

        Positioning = positioning;
        Left = left;
        Top = top;
        LeftRelational = leftRelational;
        TopRelational = topRelational;
    }
}