// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public sealed class LineCompositionBuilder : IUIElementBuilder<LineComposition>
{
    private readonly List<LineChild> _lines = new();

    public Color Color { get; init; } = Color.Default;

    public LineKind LineKind { get; init; } = LineKind.Single;
    
    public LineCharSet? LineCharSet { get; init; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public Size Size { get; }

    public LineCompositionBuilder Add(int length, Orientation orientation, int left, int top)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), length, null);
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), left, null);
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), top, null);
        
        _lines.Add(new LineChild(length, orientation, left, top));
        
        return this;
    }

    public LineComposition Build(UIElementBuildArgs args)
    {
        int width = args.Width;
        int height = args.Height;

        var charSet = LineCharSet ?? LineCharSets.Of(LineKind);
        
        var result = new LineComposition(width, height, _lines.ToArray(), charSet, OverlappingPriority)
        {
            Color = Color,
        };

        return result;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);
    
    // There are no no ability to make line composition with relational size in order to ease it.
    // What's more, It's used in cases when width and height are already known.
    public LineCompositionBuilder(int width, int height)
    {
        Size = new Size(width, height);
    }
}