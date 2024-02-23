using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

[Flags]
public enum Sizing
{
    Absolute,
    RelationalHeight = 1,
    RelationalWidth = 2,
    Relational = RelationalHeight | RelationalWidth,
}

public sealed class Size
{
    public Sizing Sizing { get; }
        
    public int? Height { get; } 
        
    public int? Width { get; }
        
    public double? HeightRelation { get; }
        
    public double? WidthRelation { get; }
    
    [MemberNotNullWhen(true, nameof(HeightRelation))]
    [MemberNotNullWhen(false, nameof(Height))]
    public bool IsHeightRelational => Sizing.HasFlag(Sizing.RelationalHeight);
    
    [MemberNotNullWhen(true, nameof(WidthRelation))]
    [MemberNotNullWhen(false, nameof(Width))]
    public bool IsWidthRelational => Sizing.HasFlag(Sizing.RelationalWidth);

    public Size(int width, int height)
        : this(Sizing.Absolute, width, height, null, null)
    { }

    public Size(int width, double heightRelation)
        : this(Sizing.RelationalHeight, width, null, null, heightRelation)
    { }

    public Size(double widthRelation, int height)
        : this(Sizing.RelationalWidth, null, height, widthRelation, null)
    { }

    public Size(double widthRelation, double heightRelation)
        : this(Sizing.Relational, null, null, widthRelation, heightRelation)
    { }
    
    public Size(Sizing sizing, int? width, int? height, double? widthRelation, double? heightRelation)
    {
        if (sizing.HasFlag(Sizing.RelationalHeight))
            ArgumentNullException.ThrowIfNull(heightRelation, nameof(heightRelation));
        else
            ArgumentNullException.ThrowIfNull(height, nameof(height));
        
        if (sizing.HasFlag(Sizing.RelationalWidth))
            ArgumentNullException.ThrowIfNull(widthRelation, nameof(widthRelation));
        else
            ArgumentNullException.ThrowIfNull(width, nameof(width));

        Sizing = sizing;
        HeightRelation = heightRelation;
        WidthRelation = widthRelation;
        Width = width;
        Height = height;
    }
}