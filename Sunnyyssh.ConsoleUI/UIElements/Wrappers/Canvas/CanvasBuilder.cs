namespace Sunnyyssh.ConsoleUI;

public sealed class CanvasBuilder : IUIElementBuilder<Canvas>
{
    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();
    
    public Size Size { get; }

    public bool EnableOverlapping { get; init; } = true;

    public ConsoleKey[] FocusChangeKeys { get; init; } = new[] { ConsoleKey.Tab };
    
    public CanvasBuilder Add(UIElement element, Position position)
    {
        _orderedQueuedChildren.Add(new QueuedPositionChild(element, position));

        return this;
    }
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, position));
        
        return this;
    }
        
    public Canvas Build(UIElementBuildArgs args)
    {
        var placementBuilder = new ElementsFieldBuilder(args.Width, args.Height, EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            if (queuedChild.IsInstance)
            {
                placementBuilder.Place(queuedChild.Element, queuedChild.Position);
                continue;
            }

            placementBuilder.Place(queuedChild.Builder, queuedChild.Position);
        }

        var orderedChildren = placementBuilder.Build();

        var resultCanvas = new Canvas(args.Width, args.Height, FocusChangeKeys, orderedChildren);

        return resultCanvas;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args)
    {
        return Build(args);
    }

    public CanvasBuilder(Size size)
    {
        Size = size;
    }
}