namespace Sunnyyssh.ConsoleUI;

internal enum Positioning
{
    Absolute,
    Relational,
    RelationalTop,
    RelationalLeft,
}
internal class Position
{
    public int? Left { get; }
    public int? Top { get; }
    public double? LeftRelational { get; }
    public double? TopRelational { get; }
    public Positioning Positioning { get; }
    
    public Position(Positioning positioning, int? left, int? top, double? leftRelational, double? topRelational)
    {
        Positioning = positioning;
        Left = left;
        Top = top;
        LeftRelational = leftRelational;
        TopRelational = topRelational;
    }
}