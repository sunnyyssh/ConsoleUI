using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class LineBuilder : IUIElementBuilder<Line>
{
    private readonly Size _size;
    
    Size IUIElementBuilder.Size => _size;

    public Orientation Orientation { get; }
    
    public int? Length { get; }
    
    public double? LengthRelational { get; }
    
    [MemberNotNullWhen(true, nameof(LengthRelational))]
    [MemberNotNullWhen(false, nameof(Length))]
    public bool IsLengthRelational { get; }

    public Color Color { get; init; } = Color.Default;

    public LineKind LineKind { get; init; } = LineKind.Single;

    public LineCharSet? CharSet { get; init; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Line Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var charSet = CharSet ?? LineCharSets.Of(LineKind);

        int length = Orientation == Orientation.Horizontal ? width : height;

        var resultLine = new Line(length, Orientation, charSet, OverlappingPriority)
        {
            Color = Color
        };

        return resultLine;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public LineBuilder(int length, Orientation orientation)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), length, null);
            
        Length = length;
        IsLengthRelational = false;
        Orientation = orientation;
        
        _size = orientation switch
        {
            Orientation.Vertical => new Size(1, length),
            Orientation.Horizontal => new Size(length, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    public LineBuilder(double lengthRelational, Orientation orientation)
    {
        if (lengthRelational <= 0.0 || lengthRelational >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(lengthRelational), lengthRelational, null);
        
        LengthRelational = lengthRelational;
        IsLengthRelational = true;
        Orientation = orientation;

        _size = orientation switch
        {
            Orientation.Vertical => new Size(1, lengthRelational),
            Orientation.Horizontal => new Size(lengthRelational, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }
}