using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class BorderBuilder : IUIElementBuilder<Border>
{
    public Size Size { get; }
    
    public BorderCharSet? BorderCharSet { get; }
    
    public BorderKind? BorderKind { get; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Color Color { get; init; } = Color.Default;
    
    [MemberNotNullWhen(true, nameof(BorderKind))]
    [MemberNotNullWhen(false, nameof(BorderCharSet))]
    public bool IsOneOfKinds { get; }

    public Border Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var charSet = IsOneOfKinds ? BorderCharSets.Of(BorderKind.Value) : BorderCharSet;

        var resultBorder = new CharSetBorder(width, height, charSet, Color, OverlappingPriority);

        return resultBorder;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public BorderBuilder(int width, int height, BorderKind borderKind)
        : this(new Size(width, height), borderKind)
    { }

    public BorderBuilder(int width, double heightRelation, BorderKind borderKind)
        : this(new Size(width, heightRelation), borderKind)
    { }

    public BorderBuilder(double widthRelation, int height, BorderKind borderKind)
        : this(new Size(widthRelation, height), borderKind)
    { }

    public BorderBuilder(double widthRelation, double heightRelation, BorderKind borderKind)
        : this(new Size(widthRelation, heightRelation), borderKind)
    { }
    
    public BorderBuilder(Size size, BorderKind borderKind)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        
        if (borderKind == ConsoleUI.BorderKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(borderKind), "Border kind can't be None.");
        }
        
        Size = size;
        BorderKind = borderKind;
        IsOneOfKinds = true;
    }

    public BorderBuilder(int width, int height, BorderCharSet charSet)
        : this(new Size(width, height), charSet)
    { }

    public BorderBuilder(int width, double heightRelation, BorderCharSet charSet)
        : this(new Size(width, heightRelation), charSet)
    { }

    public BorderBuilder(double widthRelation, int height, BorderCharSet charSet)
        : this(new Size(widthRelation, height), charSet)
    { }

    public BorderBuilder(double widthRelation, double heightRelation, BorderCharSet charSet)
        : this(new Size(widthRelation, heightRelation), charSet)
    { }
    
    public BorderBuilder(Size size, BorderCharSet borderCharSet)
    {
        ArgumentNullException.ThrowIfNull(borderCharSet, nameof(borderCharSet));
        
        Size = size;
        BorderCharSet = borderCharSet;
        IsOneOfKinds = false;
    }
}