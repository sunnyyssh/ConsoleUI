using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public class BorderBuilder : IUIElementBuilder<Border>
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
        int width = args.Width;
        int height = args.Height;

        BorderCharSet charSet;
        
        if (IsOneOfKinds)
        {
            charSet = BorderKind switch
            {
                ConsoleUI.BorderKind.SingleLine => SingleLineBorder.SingleLineCharSet,
                ConsoleUI.BorderKind.DoubleLine => DoubleLineBorder.DoubleLineCharSet,
                ConsoleUI.BorderKind.Dense => DenseBorder.DenseCharSet,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            charSet = BorderCharSet;
        }

        var resultBorder = new CharSetBorder(width, height, charSet, Color, OverlappingPriority);

        return resultBorder;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args)
    {
        return Build(args);
    }

    public BorderBuilder(Size size, BorderKind borderKind)
    {
        if (borderKind == ConsoleUI.BorderKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(borderKind), "Border kind can't be None.");
        }
        Size = size;
        BorderKind = borderKind;
        IsOneOfKinds = true;
    }

    public BorderBuilder(Size size, BorderCharSet borderCharSet)
    {
        Size = size;
        BorderCharSet = borderCharSet;
        IsOneOfKinds = false;
    }
}