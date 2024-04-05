using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// The kind of size.
/// </summary>
[Flags]
internal enum Sizing
{
    Absolute,
    RelationalHeight = 1,
    RelationalWidth = 2,
    Relational = RelationalHeight | RelationalWidth,
}

/// <summary>
/// Specifies a size of <see cref="UIElement"/>.
/// </summary>
public sealed class Size
{
    /// <summary>
    /// Full size. (new Size(1.0, 1.0))
    /// </summary>
    public static readonly Size FullSize = new Size(1.0, 1.0);
    
    internal Sizing Sizing { get; }
        
    /// <summary>
    /// Absolute height. (Counted in characters).
    /// </summary>
    public int? Height { get; } 
        
    /// <summary>
    /// Absolute width. (Counted in characters).
    /// </summary>
    public int? Width { get; }
        
    /// <summary>
    /// Relational height. Can be more than 0 and less or equal to 1. (Counted from area height)
    /// </summary>
    public double? HeightRelation { get; }
        
    /// <summary>
    /// Relational width. Can be more than 0 and less or equal to 1. (Counted from area width)
    /// </summary>
    public double? WidthRelation { get; }
    
    /// <summary>
    /// True if height is relational. False if height is absolute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(HeightRelation))]
    [MemberNotNullWhen(false, nameof(Height))]
    public bool IsHeightRelational => Sizing.HasFlag(Sizing.RelationalHeight);
    
    /// <summary>
    /// True if width is relational. False if width is absolute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(WidthRelation))]
    [MemberNotNullWhen(false, nameof(Width))]
    public bool IsWidthRelational => Sizing.HasFlag(Sizing.RelationalWidth);

    /// <summary>
    /// Creates <see cref="Size"/> instance.
    /// </summary>
    /// <param name="width">Absolute width. (Counted in characters).</param>
    /// <param name="height">Absolute height. (Counted in characters).</param>
    public Size(int width, int height)
        : this(Sizing.Absolute, width, height, null, null)
    { }

    /// <summary>
    /// Creates <see cref="Size"/> instance.
    /// </summary>
    /// <param name="width">Absolute width. (Counted in characters).</param>
    /// <param name="heightRelation">
    /// Relational height. Can be more than 0 and less or equal to 1. (Counted from area height)
    /// </param>
    public Size(int width, double heightRelation)
        : this(Sizing.RelationalHeight, width, null, null, heightRelation)
    { }

    /// <summary>
    /// Creates <see cref="Size"/> instance.
    /// </summary>
    /// <param name="widthRelation">
    /// Relational width. Can be more than 0 and less or equal to 1. (Counted from area width)
    /// </param>
    /// <param name="height">Absolute height. (Counted in characters).</param>
    public Size(double widthRelation, int height)
        : this(Sizing.RelationalWidth, null, height, widthRelation, null)
    { }

    /// <summary>
    /// Creates <see cref="Size"/> instance.
    /// </summary>
    /// <param name="widthRelation">
    /// Relational width. Can be more than 0 and less or equal to 1. (Counted from area width)
    /// </param>
    /// <param name="heightRelation">
    /// Relational height. Can be more than 0 and less or equal to 1. (Counted from area height)
    /// </param>
    public Size(double widthRelation, double heightRelation)
        : this(Sizing.Relational, null, null, widthRelation, heightRelation)
    { }
    
    internal Size(Sizing sizing, int? width, int? height, double? widthRelation, double? heightRelation)
    {
        if (sizing.HasFlag(Sizing.RelationalHeight))
            ArgumentNullException.ThrowIfNull(heightRelation, nameof(heightRelation));
        else
            ArgumentNullException.ThrowIfNull(height, nameof(height));
        
        if (sizing.HasFlag(Sizing.RelationalWidth))
            ArgumentNullException.ThrowIfNull(widthRelation, nameof(widthRelation));
        else
            ArgumentNullException.ThrowIfNull(width, nameof(width));

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, null);
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, null);
        if (widthRelation <= 0.0 || widthRelation > 1.0)
            throw new ArgumentOutOfRangeException(nameof(widthRelation), widthRelation, null);
        if (heightRelation <= 0.0 || heightRelation > 1.0)
            throw new ArgumentOutOfRangeException(nameof(heightRelation), heightRelation, null);            
        
        Sizing = sizing;
        HeightRelation = heightRelation;
        WidthRelation = widthRelation;
        Width = width;
        Height = height;
    }
}