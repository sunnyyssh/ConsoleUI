namespace Sunnyyssh.ConsoleUI;

public sealed class CanvasBuilder : IUIElementBuilder<Canvas>
{
    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();
    
    public Size Size { get; }

    public bool EnableOverlapping { get; init; } = true;

    public ConsoleKeyCollection FocusChangeKeys { get; init; } = new[] { ConsoleKey.Tab }.ToCollection();
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, position));
        
        return this;
    }
        
    public Canvas Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        var placementBuilder = new ElementsFieldBuilder(args.Width, args.Height, EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            placementBuilder.Place(queuedChild.Builder, queuedChild.Position);
        }

        var orderedChildren = placementBuilder.Build();

        var resultCanvas = new Canvas(args.Width, args.Height, FocusChangeKeys, orderedChildren);

        return resultCanvas;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public CanvasBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
    }
}